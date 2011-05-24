using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
namespace WeeboxSync {

    public class FicheiroSystemAbstraction {
        /*
         * Creates a link between a tag and a bundle.
         * Link is placed in orig_path and links to dest_path
         */
        public void CreateROLink(String origPath, String destPath) {
            throw new System.Exception("Not implemented");
        }

        /*
         * Creates a folder in specified path
         * NOT YET READ ONLY
         */
        public void CreateROFolder(String path) {
            //returns quietly if dir already exists
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path); //TODO - Set folder permissions
            //make dir read only
            //DirectoryInfo myDirectoryInfo = new DirectoryInfo(path);
            //DirectorySecurity myDirectorySecurity = myDirectoryInfo.GetAccessControl();
            //myDirectorySecurity.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.Modify));
        }

        public void CreateFicheiro(Ficheiro ficheiro, String path) {
            throw new System.Exception("Not implemented");
            
        }

        /*
         * deletes an empty folder
         */
        public void DeleteFolder(String path) {
            if (IsDirectoryEmpty(path))
            {
                try
                {
                    Directory.Delete(path);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        //delete a folder and all its files and sub-folders
        public void DeleteRecursiveFolder(string path)
        {
            //if (IsDirectoryEmpty(path))
            //    return;

            DirectoryInfo parent = new DirectoryInfo(path);

            parent.Delete(true); //recursive delete

            //foreach (var dir in parent.EnumerateDirectories())
            //{
            //    DeleteRecursiveFolder(path + "\\" + dir.Name);
            //}

            //foreach (var file in parent.EnumerateFiles())
            //{
            //    file.Delete();
            //}

            //parent.Delete();
            
        }

        //determine if a directory is empty and eligible for deletion
        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        //Deletes all link to and files from a bundle and its folder representation
        public void DeleteBundleFromFS(Bundle bundle)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(bundle.localId);

            foreach (var file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            dirInfo.Delete();
        }

        public List<String> GetFicheiroIDSFromFolder(String path) {
            throw new System.Exception("Not implemented");
        }

        public void DeleteFicheiroFromFS(String fpath) {
            throw new System.Exception("Not implemented");
        }

        public void SaveFicheiroInFS(String path, Ficheiro ficheiro) {
            throw new System.Exception("Not implemented");
        }

    }
}

