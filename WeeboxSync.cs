using System;
using Microsoft.Http; 
namespace WeeboxSync {
    public class WeeboxSync {
        public ConnectionInfo connection_info { get; set;  }

        public void GetAllBundles() {
            throw new System.Exception("Not implemented");
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