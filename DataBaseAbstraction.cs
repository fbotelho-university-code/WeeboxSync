using System;
using System.Collections.Generic;
namespace WeeboxSync {
    public class DataBaseAbstraction {
        public void SaveBundle(Bundle bundle) {
            throw new System.Exception("Not implemented");
        }

        public void DeleteBundle(String bundleId) {
            throw new System.Exception("Not implemented");
        }


        public void SaveFicheiroInfo(Ficheiro Ficheiro, String bundleId) {
            throw new System.Exception("Not implemented");
        }
        public void GetClassificationScheme() {
            throw new System.Exception("Not implemented");
        }
        public void SaveClassificationScheme(Scheme scheme) {
            throw new System.Exception("Not implemented");
        }
        public void UpdateClassificationScheme(Scheme scheme) {
            throw new System.Exception("Not implemented");
        }
        public void SaveConnectionInfo(ConnectionInfo connectionInfo) {
            throw new System.Exception("Not implemented");
        }

        public void SaveRootFolder(String path) {
            throw new System.Exception("Not implemented");
        }
        public List<String> GetAllBundles() {
            throw new System.Exception("Not implemented");
        }
        public void RmBundle(String bundleId) {
            throw new System.Exception("Not implemented");
        }
        public void GetFicheirosIDS(String bundleId) {
            throw new System.Exception("Not implemented");
        }
        public void RmFicheiroInfo(String Ficheiro, String bundleID) {
            throw new System.Exception("Not implemented");
        }
        public void SaveConnection(ConnectionInfo con) {
            throw new System.Exception("Not implemented");
        }
        public ConnectionInfo ReadConnectionInfo(String username) {
            throw new System.Exception("Not implemented");
        }

    }
}