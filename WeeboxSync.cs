
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WeeboxSync {
    public class WeeboxSync
    {
        public ConnectionInfo connection_info { get; set;  }
        private  long bundle_serial_generator =0;
        private IEnumerable<Scheme> scheme; 
        private  String root_folder = null;
        private CoreAbstraction core;
        private FicheiroSystemAbstraction fileSystem; 
        public String default_root_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
        private string path_schemes= null;
        private string path_bundles= null;
        public WeeboxSync(){
            core = new CoreAbstraction();
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

            IEnumerable<Scheme> schemes = core.getSchemesFromServer();
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
        public void SetRootFolder(object folder) {
            throw new System.Exception("Not implemented");
        }
        public void SetDefaultRootFolder() {
            throw new System.Exception("Not implemented");
        }
        public void InitSetup() {
            throw new System.Exception("Not implemented");
        }
        public void SyncBundle(object bundleId) {
            throw new System.Exception("Not implemented");
        }
        public void SyncBundles() {
            throw new System.Exception("Not implemented");
        }


        public bool CreateBundle(string bundleId_server_id){
            //deve garantir que os planos de classificação estão actualizados 
            string path_bundle = this.path_bundles + "\\" + this.bundle_serial_generator;
            Directory.CreateDirectory(path_bundle); 
            Bundle b = core.getBundle(bundleId_server_id, path_bundle);
            b.localId = "" + this.bundle_serial_generator;
            this.bundle_serial_generator += 1; 
            if (b.weeTags != null){
   
                foreach (String t in b.weeTags){
                    Tag tag = this._getTagByWeeIds(t);
                    if (tag != null){
                        //                   fileSystem.CreateROLink(path_bundle, tag.Path);
                        fileSystem.CreateROLink(this.path_schemes + "\\" + tag.Path, path_bundle + "\\", b.localId);
                    }
                }
            }
            return true; //has created bundle
        }

        private bool _enforcePresence_of_tags(Bundle b){
                bool value = false;
                foreach (string tagId in b.weeTags){
                    foreach (Scheme s in scheme){
                        value = s.arvoreByWeeboxIds.find(tagId) != null;
                        if (value) return true;
                    }
                }
            return false; 
        }


        private Tag _getTagByWeeIds(string id){
            foreach (Scheme s in this.scheme){
                Tag t = s.arvoreByWeeboxIds.find(id); 
                if (t != null) return t; 
            }
            return null; 
        }

        private ConnectionInfo connectionInfo;
        private int MAX_SYNC_TRYS= 10;
    }


}