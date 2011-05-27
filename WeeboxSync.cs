
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WeeboxSync {
    public class WeeboxSync
    {
        public ConnectionInfo connection_info { get; set;  }
        private  long bundle_serial_generator =0;
        private List<Scheme> scheme; 
        private  String root_folder = null;
        private CoreAbstraction core;
        private FicheiroSystemAbstraction fileSystem; 
        public String default_root_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
        private String path_schemes= null;
        private String path_bundles= null;
        public WeeboxSync(){
            core = CoreAbstraction.getCore();
            fileSystem = new FicheiroSystemAbstraction();
        }

        public String getRootFolder(){
            return root_folder; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// a handfull of exceptions
        /// <param name="s"></param>
        /// <returns></returns>
        public bool setRootFolder(string s){
            string path = s + "\\My Weebox";
            string path_Schemes = path + "\\Schemes";
            string path_bundles = path + "\\Bundles"; 
            if (!Directory.Exists(path)){
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path_Schemes);
                Directory.CreateDirectory(path_bundles); 
            }
//            else throw new ArgumentOutOfRangeException();
            this.root_folder = path;
            this.path_schemes = path_Schemes;
            this.path_bundles = path_bundles; 
            return true;
        }

        public void setDefaultRootFolder(){
            this.setRootFolder(this.default_root_folder); 
        }

        public void setup(){
            core.SetConnection(this.connection_info);
            this.setDefaultRootFolder();

            List<Scheme> schemes = core.getSchemesFromServer();
            this.scheme = schemes; 
            this.setDefaultRootFolder();

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
                             Console.Error.WriteLine("Abruptley terminate program");
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
                                 Console.WriteLine("Abruptley terminate program");
                             }
                         }
                     }
                 }
             }


             //update classification scheme in bd 
             scheme = newScheme;
         }

        public void syncBundles(){
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


            public bool syncBundles(String bundleId){
         
                return true;      // TOdo read from db Bundle and call syncBundle(Bundle)
            }

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


        private ConnectionInfo connectionInfo;
    }


}