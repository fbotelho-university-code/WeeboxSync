
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
        private DataBaseAbstraction dataBase; 
        private String path_schemes= null;
        private String path_bundles= null;

        private readonly object SyncLock = new object ();
        private readonly object BagLock = new object (); //controls access to the collection of to update bundle
        private List<String> bundlesToUpdate;

        public WeeboxSync(){
            core = CoreAbstraction.getCore();
            fileSystem = new FicheiroSystemAbstraction();
            dataBase = new DataBaseAbstraction(); 
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
                //TODO - tocha - try to sync if queue empty
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

                foreach (Scheme sch in schemes) {
                    Tag root = sch.arvore.getRoot();
                    Directory.CreateDirectory(this.path_schemes + "\\" + root.Path);
                    if (root != null) {
                        _createFolderForTag(root, sch);
                    }
                }

                // save Schemes in bd 
                IEnumerable<String> bundles = core.GetAllBundlesList();
                foreach (String weeId in bundles) {
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
             dataBase.SaveClassificationScheme(scheme);
         }

        private void syncBundles(){
             // sync bundles 
             List<String> allBundlesInServer = core.GetAllBundlesList();
             //get all bundles in bd 
             List<Bundle> allBundlesInFS = dataBase.GetAllBundles(); 

             foreach (Bundle bundle in allBundlesInFS){
                 String lastest_version = syncBundle(bundle);
                 if (lastest_version != null){
                     allBundlesInServer.Remove(lastest_version);
                 }
             }
             foreach(String id in allBundlesInServer){
                 CreateBundle(id); 
             }
         }
        /// <summary>
        /// Synchronize one bundle
        /// </summary>
        /// <param name="bundleId">The LOCAL ID of the bundle to be synchronized</param>
        /// <returns></returns>
        public string syncBundle(String bundleId) {
            return syncBundle (dataBase.GetBundle (bundleId));
        }
        /// <summary>
        /// Synchronize one bundle
        /// </summary>
        /// <param name="bundle">The bundle to be synchronized</param>
        /// <returns></returns>
        public string syncBundle(Bundle bundle){
            String bundle_lastest_version_id = core.GetLatestVersionIdFromServer(bundle.weeId);
            if (bundle_lastest_version_id == null){
                //bundle foi eliminado do servidor
                //delete bundle from db 
                dataBase.DeleteBundle(bundle.localId);

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
                return null;
                }
                if (bundle_lastest_version_id == bundle.weeId){
                    //Bundle doesn't has a new version on server
                _sync_with_no_new_version(bundle);
                return bundle_lastest_version_id;
            }
            else{
                //bundle has a new version on server
                _sync_with_new_version(bundle);
                return bundle_lastest_version_id;
            }
        }


        private bool _sync_with_new_version(Bundle bundle){
            //try catch 
            List<Ficheiro> filesFS = fileSystem.getFicheirosFromFolder(bundle.getPath(path_bundles), bundle.localId);
            Bundle bInfo = core.GetLatestVersionFromServer(bundle.weeId);
            if (bInfo == null) return false; // WE SHOULD NOT EVEN BE HERE , DOESNT HAS A NEW VERSION ON SERVER. 
            List<Ficheiro> filesCore = bInfo.filesPath;
            List<Ficheiro> filesSync = dataBase.GetFicheirosIDS(bundle.localId);

            String transformedBundleId = bInfo.weeId;
            // where whe apply the modifications and save the new bundle wee id.

            //Nomes diferentes
            transformedBundleId = _alterFileNames(ref bundle, ref filesFS, ref filesCore, ref filesSync,
                                                  transformedBundleId);

            List<Ficheiro> temp = new List<Ficheiro>(filesFS);
            // updates nos dois lados , fica com os dois TODO
            foreach (Ficheiro fileSystemFile in filesFS){
                Ficheiro fileSynced = filesSync.Find((Ficheiro x) => x.md5 == fileSystemFile.md5);
                Ficheiro fileCored = filesCore.Find((Ficheiro x) => x.md5 == fileSystemFile.md5);
                bool duplicate_copy = false;
                if (fileSynced == null && fileCored == null){
                    Ficheiro fileSyncedSameName = filesSync.Find((Ficheiro x) => x.name == fileSystemFile.name);
                    Ficheiro fileCoredSameName = filesCore.Find((Ficheiro x) => x.name == fileSystemFile.name);

                    if (fileCoredSameName != null){
                        //ficheiro estava sincronizado 
                        if (fileSyncedSameName != null){
                            if (fileSyncedSameName.md5 != fileCoredSameName.md5){
                                duplicate_copy = true;
                            }
                        }
                        else{
                            duplicate_copy = true;
                        }
                    }
                    core.PutFicheiro(fileFS);
                    core.GetFicheiro(coreFIle);
                    // bd remove antigo (se existir) , adiciona novos 2. 
                    //remover das listas

                    filesCore.RemoveAll((Ficheiro x) => x.name == fileSystemFile.name);
                    filesSync.RemoveAll((Ficheiro x) => x.name == fileSystemFile.name);

                    temp.Remove(fileSystemFile);
                }
            }

            filesFS = temp;


            foreach (Ficheiro fileSystemFile in filesFS){
                Ficheiro fileSynced = filesSync.Find((Ficheiro x) => x.md5 == fileSystemFile.md5);
                Ficheiro fileCored = filesCore.Find((Ficheiro x) => x.md5 == fileSystemFile.md5);

                // Ficheiros só presentes no file system 
                if (fileSynced == null && fileCored == null){
                    //novo ficheiro
                    Ficheiro coreSameNameFile = filesCore.Find((Ficheiro x) => x.name == fileSystemFile.name);
                    Ficheiro bdSameNameFile = filesSync.Find((Ficheiro x) => x.name == fileSystemFile.name);

                    if (coreSameNameFile != null){
                        Tuple<String, List<String>> removedInfo = core.RemoveFicheiro(transformedBundleId,
                                                                                      coreSameNameFile.md5);
                        transformedBundleId = removedInfo.Item1;
                        dataBase.UpdateWeeId(bundle.localId, transformedBundleId);
                        filesCore.Remove(coreSameNameFile);
                    }

                    if (bdSameNameFile != null){
                        dataBase.DeleteFicheiroInfo(bdSameNameFile.md5, bdSameNameFile.bundleId);
                        filesSync.Remove(bdSameNameFile);
                    }

                    //adicionar ao core
                    transformedBundleId = core.PutFicheiro(transformedBundleId, fileSystemFile);
                    dataBase.UpdateFicheiroInfo(fileSystemFile);
                    dataBase.UpdateWeeId(bundle.localId, transformedBundleId);

                    //apagar md5 da lista actual , nome das outras
                }

                // id do ficheiro só nao existe no core
                if ((fileSynced != null) && (fileCored == null)){
                    try{
                        fileSystem.DeleteFile(fileSystemFile.path);
                        dataBase.DeleteFicheiroInfo(fileSynced.md5, fileSynced.bundleId);
                        //caguei de alto
                    }
                    catch (Exception e){
                        //add bundle to updateQueue
                        continue;
                    }
                    //remover file da lista md5 actual, nomes dos outros. 
                }
            }

            foreach (Ficheiro fileCored in filesCore){
                Ficheiro fileFS = filesFS.Find((Ficheiro x) => x.md5 == fileCored.md5);
                Ficheiro fileSync = filesSync.Find((Ficheiro x) => x.md5 == fileCored.md5);

                if ((fileFS == null) && (fileSync == null)){
                    //File só  existe no bundle do core
                    Ficheiro fileSameNameFS = filesFS.Find((Ficheiro x) => x.name == fileCored.name);
                    Ficheiro fileSameNameSynced = filesSync.Find((Ficheiro x) => x.name == fileCored.name);
                    string file_path = bundle.getPath(path_bundles) + "\\" + fileCored.name;
                    //se fileSameNameFS != fileSameNameSynced  duplicate copy porque sao duas versoes diferentes 


                    //TODO WARNING, não estamos a actualizar o file system se não existir um ficheiro com o mesmo nome 
                    if (fileSameNameFS){
                        //file already exits. md5 differs, 
                        //try remove it or change is name.
                        Boolean deleted;
                        try{
                            fileSystem.DeleteFile(file_path);
                            deleted = true;
                        }
                        catch (IOException e){
                            deleted = false;
                        }

                        if (!deleted){
                            continue;
                        }
                        else{
                            try{
                                core.GetFicheiro(transformedBundleId, fileCored.md5, file_path);
                                //update bd 
                            }
                            catch (Exception e){
                                continue;
                            }
                        }
                    }

                }

                if ((fileSync != null) && (fileFS == null)){
                    try{
                        core.RemoveFicheiro(transformedBundleId, fileCored.md5);
                        //update bd. 
                    }
                    catch (Exception e){
                        continue;
                    }
                }
                //remove md5 from core list and remove same name from every list

                foreach (File file_garbage in filesSync){
                    //db.remove file 
                }
            }
            return true; 
        }

        private
            String _alterFileNames
            (ref Bundle bundle, ref List<Ficheiro> filesFS, ref List<Ficheiro> filesCore, ref List<Ficheiro> filesSync,
             string bundleId)
            {
                List<Ficheiro> copyFilesCore = filesCore;
                String transformedBundleID = bundleId;
                List<Ficheiro> newNameFiles =
                    filesFS.FindAll(
                        (Ficheiro fileFS) =>
                        copyFilesCore.Exists(
                            (Ficheiro coreFile) => coreFile.md5 == fileFS.md5 && coreFile.name != fileFS.name));

                foreach (Ficheiro newNameFile in newNameFiles){
                    //ver quem é que mudou de nome 
                    Ficheiro inFileSys = newNameFile;
                    Ficheiro inSyncedFile = filesSync.Find((Ficheiro x) => x.md5 == inFileSys.md5);
                    Ficheiro inCoreFile = filesCore.Find((Ficheiro x) => x.md5 == inFileSys.md5);
                    if (inSyncedFile == null ||
                        ((inSyncedFile.name != inCoreFile.name) && inSyncedFile.name != inFileSys.name)){
                        // files wasn't synced , we don't have a way to find out who was the last one to be renamed.  OR  file changed in both places 
                        // policy : stay with server name , faster solution 
                        Ficheiro newFile = new Ficheiro(bundle.getPath(path_bundles) + "\\" + inCoreFile.name,
                                                        bundle.localId, inFileSys.md5);
                        try{
                            fileSystem.RenameFile(inFileSys.path, newFile.path);
                        }
                        catch (Exception e){
                            // We could not rename the file 
                            //TODO - Try change in server , might be cool 
                            //ignoring file 
                            continue;
                        }
                    }
                    else{
                        if ((inSyncedFile.name == inCoreFile.name)){
                            //mudou de nome só localmente. 
                            try{
                                //TODO save changes to database
                                Tuple<String, List<String>> removedInfo = core.RemoveFicheiro(bundle.weeId,
                                                                                              newNameFile.md5);
                                if (removedInfo.Item2.Count != 1){
                                    throw new SystemException("Could not update, being catched right below");
                                }
                                transformedBundleID = removedInfo.Item1;
                                transformedBundleID = core.PutFicheiro(transformedBundleID, newNameFile);
                                // might throw another exception being catch below
                            }
                            catch (Exception e){
                                //we could not rename the file continue with sync; 
                                continue; //outer loop , syncing other files. 
                            }

                        }
                        else{
                            //file change remotly rename local file. 
                            Ficheiro newFile = new Ficheiro(bundle.getPath(path_bundles) + "\\" + inCoreFile.name,
                                                            bundle.localId, inFileSys.md5);
                            try{
                                fileSystem.RenameFile(inFileSys.path, newFile.path);
                            }
                            catch (Exception e){
                                // We could not rename the file 
                                //TODO - Try change in server , might be cool 
                                //ignoring file 
                                continue;
                            }
                        }
                    }

                }
                return transformedBundleID;
            }

        private bool _sync_with_no_new_version(Bundle bundle){
            List<Ficheiro> filesFs;
            List<Ficheiro> filesLastSync; 
            try{
                filesFs = fileSystem.getFicheirosFromFolder(bundle.getPath(path_bundles), bundle.localId);
                filesLastSync = dataBase.GetFicheirosIDS(bundle.localId);
            }catch(Exception e ){
                return false; 
            }

            //TODO - speed things up , create a list foreach case.
            if (filesFs==null || filesLastSync == null ){
                return false; 
            }

            String bundleTransformedId = bundle.weeId;
            //ficheiros que mudaram de nome 
            //if files exists in both bundles but has different name 
            List<Ficheiro> newNameFiles =
                filesFs.FindAll(
                    (Ficheiro fsFile) =>
                    filesLastSync.Exists(
                        (Ficheiro syncFile) => syncFile.md5 == fsFile.md5 && syncFile.name != fsFile.name));
            foreach (Ficheiro newNameFile in newNameFiles){
                //eliminar e adicionar ficheiro é a unica forma de renomear o ficheiro
                Ficheiro syncFile = filesLastSync.Find((Ficheiro x) => x.md5 == newNameFile.md5);
                Tuple<String, List<String>> removedInfo = core.RemoveFicheiro(bundle.weeId, newNameFile.md5);
                bundleTransformedId = removedInfo.Item1; 
                dataBase.UpdateWeeId(bundle.localId, bundleTransformedId);
                if (removedInfo.Item2.Count == 0){
                    dataBase.DeleteFicheiroInfo(newNameFile.md5, bundle.localId); 
                    bundleTransformedId = core.PutFicheiro(bundleTransformedId, newNameFile);
                    dataBase.UpdateWeeId(bundle.localId, bundleTransformedId);                    
                    dataBase.SaveFicheiroInfo(newNameFile);
                }
            }
            // ficheiros só no file system
            foreach (Ficheiro file in filesFs){
                //if file only exists in file system 
                if (!filesLastSync.Exists((Ficheiro syncFile) => syncFile.md5 == file.md5)){
                    try{
                        Ficheiro fileCored = filesLastSync.Find((Ficheiro x) => x.name == file.name); 
                        // if fileCored exits it has the same name , we have to removed beacause it's an update of the file
                        // se calhar devias verificar no core primeiro :S
                        if (fileCored != null) {
                            Tuple<String, List<String>> removedInfo = core.RemoveFicheiro (bundleTransformedId,
                                                                                           fileCored.md5);
                            bundleTransformedId = removedInfo.Item1;
                            dataBase.UpdateWeeId (bundle.localId, bundleTransformedId);
                            if (removedInfo.Item2.Count == 0) {
                                dataBase.DeleteFicheiroInfo (fileCored.md5, fileCored.bundleId);
                            }
                        }
                        bundleTransformedId = core.PutFicheiro (bundleTransformedId, file);
                        dataBase.UpdateWeeId (bundle.localId, bundleTransformedId);
                        dataBase.SaveFicheiroInfo (file);
                    }catch(Exception e){
                        Console.WriteLine(e);
                        continue; 
                    }
                }
            }

            foreach (Ficheiro file in filesLastSync){
                // file only exists in last synced version
                if (!filesFs.Exists((Ficheiro fileFs) => fileFs.md5 == file.md5)){
                    try{
                        Tuple<String, List<String>> removedInfo = core.RemoveFicheiro(bundleTransformedId, file.md5);
                        bundleTransformedId = removedInfo.Item1;
                        if (removedInfo.Item2.Count == 0){
                            dataBase.DeleteFicheiroInfo(file.md5, file.bundleId);
                        }
                        dataBase.UpdateWeeId(bundle.localId, bundleTransformedId);
                    }catch(Exception e){
                        Console.WriteLine(e);
                        continue; 
                    }
                }
            }
            return true;
        }

        public bool CreateBundle(String bundleId_server_id){
            //deve garantir que os planos de classificação estão actualizados 
            string path_bundle = this.path_bundles + "\\" + this.bundle_serial_generator;
            Directory.CreateDirectory(path_bundle); 
            Bundle b = core.getBundle(bundleId_server_id, path_bundle);
            if (b == null) return false; 
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
            dataBase.SaveBundle(b);
            return true; //has created bundle
        }
                public void GetNewBundles() {
            throw new System.Exception("Not implemented");
        }

        public void TestaConexao(object server, object porta, object proxy) {
            throw new System.Exception("Not implemented");
        }

    }


}