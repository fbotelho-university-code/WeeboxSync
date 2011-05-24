
using System;
using System.Collections.Generic;
using System.IO;

namespace WeeboxSync {
    public class WeeboxSync {
        public ConnectionInfo connection_info { get; set;  }
        private  String root_folder = null; 
        public String default_root_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
        private string path_schemes= null;
        private string path_bundles= null;

        public WeeboxSync(){
            
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
            else throw new ArgumentOutOfRangeException();
            this.root_folder = path;
            this.path_schemes = path_Schemes;
            this.path_bundles = path_bundles; 
            return true;
        }

        public void setDefaultRootFolder(){
            this.setRootFolder(this.default_root_folder); 
        }
        public void setup(){
            CoreAbstraction core = new CoreAbstraction();
            core.SetConnection(this.connection_info);
            
/**            IEnumerable <Scheme>  schemes = core.getSchemesFromServer();
            this.setDefaultRootFolder(); 
            foreach (Scheme scheme in schemes){
                Tag root = scheme.arvore.getRoot();
                Directory.CreateDirectory(this.path_schemes + "\\" + root.Path); 
                if (root != null){
                  _createFolderForTag(root, scheme );
                }
            }
            */
            core.GetAllBundlesList(); 
        }

        private void _createFolderForTag(Tag node, Scheme scheme){
            Directory.CreateDirectory(this.path_schemes + "\\" + node.Path); 
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
        public void CreateBundle(object bundleId_server_id) {
            throw new System.Exception("Not implemented");
        }
        private ConnectionInfo connectionInfo;
    }
}