using System;
using System.Collections.Generic;
namespace WeeboxSync {
    public class FicheiroSystemAbstraction {
        public void CreateROLink(String orig_path, String dest_path) {
            throw new System.Exception("Not implemented");
        }
        public void CreateROFolder(String path) {
            throw new System.Exception("Not implemented");
        }

        public void CreateFicheiro(Ficheiro Ficheiro, String path) {
            throw new System.Exception("Not implemented");
        }
        public void DeleteFolder(String path) {
            throw new System.Exception("Not implemented");
        }
        public void DeleteBundleFromFS(Bundle bundle) {
            throw new System.Exception("Not implemented");
        }
        public List<String> GetFicheiroIDSFromFolder(String path) {
            throw new System.Exception("Not implemented");
        }
        public void DeleteFicheiroFromFS(String fpath) {
            throw new System.Exception("Not implemented");
        }
        public void SaveFicheiroInFS(String path, Ficheiro Ficheiro) {
            throw new System.Exception("Not implemented");
        }

    }
}