
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace WeeboxSync {
    public class WeeboxSync
    {
        public ConnectionInfo connection_info { get; set;  }
        public String default_root_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
        public int DefaultSyncInterval { get; set; } //in minutes
        
        private  long bundle_serial_generator = 0;
        private List<Scheme> scheme; 
        private  String root_folder = null;
        private CoreAbstraction core;
        private FicheiroSystemAbstraction fileSystem; 
        private String path_schemes= null;
        private String path_bundles= null;
        private DataBaseAbstraction dataBase;
        private readonly object SyncLock = new object ();
        private readonly object BagLock = new object (); //controls access to the collection of to update bundle
        private List<String> bundlesToUpdate;

        public WeeboxSync(){
            core = CoreAbstraction.getCore();
            fileSystem = new FicheiroSystemAbstraction();
            dataBase = new DataBaseAbstraction ();
            bundlesToUpdate = new List<string> ();
        }
        /// <summary>
        /// Adds a bundle to be updated when possible
        /// </summary>
        /// <param name="bundleID">The local ID of the bundle to update</param>
        public void AddBundleToUpdateQueue(string bundleID)
        {
            Monitor.Enter(BagLock);
            try
            {
                //só adicionamos o bundle se ele não estiver à espera de ser sincronizado
                if(!bundlesToUpdate.Contains (bundleID))
                    bundlesToUpdate.Add(bundleID);
            }
            finally {
                Monitor.Exit (BagLock);
            }
        }
        /// <summary>
        /// Attempts to synchronize this weebox instance
        /// </summary>
        /// <returns>true if the instance was synchronized, false if the synchronize process is already ongoing</returns>
        public bool SynchronizeAll()
        {
            //try to acquire lock and exit if 1 millisecond passes without it happening
            if (Monitor.TryEnter(SyncLock, 1))
            {//lock acquired
                try {
                    Thread t = new Thread (syncBundles) {IsBackground = true};
                    t.Start ();
                } finally {
                    Monitor.Exit (SyncLock);
                }
            }
            else {
                //acquire failed
                return false;
            }
            return true;
        }
        private void SyncQueuedBundles()
        {
            Monitor.Enter(BagLock);
            try
            {
                foreach (var bundleID in bundlesToUpdate) {
                    syncBundle (bundleID);
                }
            }
            finally {
                Monitor.Exit (BagLock);
            }
        }
        public String getRootFolder(){
            return root_folder; 
        }

        public void saveRootFolder(string root) {
            root_folder = root;
            path_schemes = root + "\\Schemes";
            path_bundles = root + "\\bundles";
        }

        /// <summary>
        /// Sets and creates the root folders of this weebox instance
        /// </summary>
        /// a handfull of exceptions
        /// <param name="s">The path where to create the root folders</param>
        /// <returns></returns>
        public bool setRootFolder(string s){
            string path = s + "\\My Weebox";
            string path_Schemes = path + "\\Schemes";
            string path_bundles = path + "\\Bundles"; 
            if (!Directory.Exists(path)){
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path_Schemes);
                Directory.CreateDirectory(path_bundles);

                this.root_folder = path;
                this.path_schemes = path_Schemes;
                this.path_bundles = path_bundles;
                return true;
            }
            else {
                this.root_folder = "";
                this.path_schemes = "";
                this.path_bundles = "";
                return false;     
            }
        }

        public void setDefaultRootFolder(){
            this.setRootFolder(this.default_root_folder); 
        }
        /// <summary>
        /// Downloads all the schemes and bundles in the server. All info needed for the process should be set
        /// </summary>
        public void setup(){
            core.SetConnection(this.connection_info);

            List<Scheme> schemes = core.getSchemesFromServer();
            this.scheme = schemes; 

            foreach (Scheme sch in schemes){
                Tag root = sch.arvore.getRoot();
                Directory.CreateDirectory(this.path_schemes + "\\" + root.Path);
                if (root != null){
                    _createFolderForTag(root, sch);
                }
            }

            IEnumerable<String> bundles = core.GetAllBundlesList();
            foreach (String weeId in bundles ){
                CreateBundle(weeId);
            }
        } 

        private void _createFolderForTag(Tag node, Scheme scheme){
            Directory.CreateDirectory(this.path_schemes + "\\" + node.Path  + "\\"); 
            IEnumerable<Tag>   filhos   = scheme.arvore.findChilds(node.Path); 
            if (filhos != null){
                foreach (Tag t in filhos){
                    _createFolderForTag(t, scheme);
                }
            }
        }

        public void GetNewBundles() {
            throw new System.Exception("Not implemented");
        }

        public void TestaConexao(object server, object porta, object proxy) {
            throw new System.Exception("Not implemented");
        }
        
         public  void syncScheme(){
             List<Scheme> newScheme = core.getSchemesFromServer();
             foreach (Scheme s in newScheme){
                 foreach (Tag t in s.arvore.getAllValue()){
                     if (Scheme.containsLocal(t.Path, this.scheme))
                         try{
                             fileSystem.CreateROFolder(t.Path);
                         }
                         catch (Exception e){
                             Console.Error.WriteLine("Abruptly terminate program");
                             Console.Read();
                         }
                 }
             }

             foreach (Scheme s in scheme){
                 foreach (Tag t in s.arvore.getAllValue()){
                     if (!Scheme.containsLocal(t.Path, newScheme)){
                         //local tag has been removed 
                         if (Directory.Exists(t.Path)){
                             try{
                                 fileSystem.DeleteRecursiveFolder(t.Path);
                             }
                             catch (Exception e){
                                 Console.WriteLine("Abruptly terminate program");
                             }
                         }
                     }
                 }
             }


             //update classification scheme in bd 
             scheme = newScheme;
         }

        private void syncBundles(){
             // sync bundles 
            List<String> allBundlesInServer = core.GetAllBundlesList(); 
            //get all bundles in bd 
            List<Bundle> allBundlesInFS= new List<Bundle>(); //= bd.getAllBundles
            //            List<String> allBundlesInBD = 
            foreach (Bundle bundle in allBundlesInFS) 
                 if (  syncBundle(bundle) == false){
                     //bundle has been removed from file system 
                     //TODO - create Folder 
                 }
        }
        /// <summary>
        /// Synchronize one bundle
        /// </summary>
        /// <param name="bundleId">The LOCAL ID of the bundle to be synchronized</param>
        /// <returns></returns>
        public bool syncBundle(String bundleId) {
            return syncBundle (dataBase.GetBundle (bundleId));
        }
        /// <summary>
        /// Synchronize one bundle
        /// </summary>
        /// <param name="bundle">The bundle to be synchronized</param>
        /// <returns></returns>
        public bool syncBundle(Bundle bundle){
            String bundle_lastest_version_id = core.GetLatestVersionIdFromServer(bundle.weeId);
            {
                if (bundle_lastest_version_id == null){
                    //bundle foi eliminado do servidor
                    //deleteBundleFromBD
                    //delete bundle from FS
                    fileSystem.DeleteRecursiveFolder(bundle.getPath(path_bundles));

                    //deleting all the links to the bundle
                    foreach (String t in bundle.weeTags){
                        Tag tag;
                        if ((tag = Scheme.getTagByWeeIds(t, scheme)) != null){
                            fileSystem.DeleteFile(tag.Path + "\\" + bundle.localId + ".lnk"); //tags exists 
                        }
                        //else .... -> if the tags have been removed we don't care.
                    }
                    return false; 
                }
                if (bundle_lastest_version_id == bundle.weeId){
                    //Bundle doesn't has a new version on server
                    sync_with_no_new_version(bundle);
                    return true; 
                }
                else{
                    //bundle has a new version on server
                    sync_with_new_version(bundle);
                    return true; 
                }
                return true; 
            }
        }

        private void sync_with_new_version(Bundle bundle){


        }

        private void sync_with_no_new_version(Bundle bundle){
           List<Tuple<String,String>> md5s_fs =  fileSystem.GetFicheiroIDSFromFolder(bundle.getPath(path_bundles));
           List<Tuple<String,String>>  md5s_from_last_updated_bundle= new List<Tuple<String,String>>(); //bd.getFilesIds.FromLastUpdatedBundles
            foreach (Tuple<String,String> md in md5s_fs){
                if (!md5s_from_last_updated_bundle.Contains(md)){
                    // ficheiro só existe no file system 
                    //core.PutFicheiro();
                }

            }
        }



        public bool CreateBundle(String bundleId_server_id){
            //deve garantir que os planos de classificação estão actualizados 
            string path_bundle = this.path_bundles + "\\" + this.bundle_serial_generator;
            Directory.CreateDirectory(path_bundle); 
            Bundle b = core.getBundle(bundleId_server_id, path_bundle);
            b.localId = "" + this.bundle_serial_generator;
            this.bundle_serial_generator += 1; 
            if (b.weeTags != null){
                foreach (String t in b.weeTags){
                    Tag tag = Scheme.getTagByWeeIds(t, this.scheme);
                    if (tag != null){
                        //                   fileSystem.CreateROLink(path_bundle, tag.Path);
                        fileSystem.CreateROLink(this.path_schemes + "\\" + tag.Path, path_bundle + "\\", b.localId);
                    }
                }
            }
            return true; //has created bundle
        }
    }


}