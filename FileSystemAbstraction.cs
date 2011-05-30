using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using IWshRuntimeLibrary;
using File = IWshRuntimeLibrary.File;

namespace WeeboxSync {
    using System.Net.Mime;
    using System.Windows.Forms;

    public class FicheiroSystemAbstraction {
        /*
         * Creates a link between a tag and a bundle.
         * Link is placed in orig_path and links to dest_path
         */
        public void CreateROLink(String origPath, String destPath, string bundleName) {
            //TODO add shortcut icon
            IWshShell_Class shell = new IWshShell_Class();
            try {
                // Create the shortcut and choose the path for the shortcut
                IWshShortcut myShortcut = shell.CreateShortcut(origPath + "\\" + bundleName + ".lnk");
                // Where the shortcut should point to
                myShortcut.TargetPath = destPath;
                // Description for the shortcut
                myShortcut.Description = "bundle";
                // Location for the shortcut's icon
                //myShortcut.IconLocation = MediaTypeNames.Application.StartupPath + @"\app.ico";
                
                myShortcut.IconLocation = Application.StartupPath + @"\Icons\bundle.ico";
                // Create the shortcut at the given path
                myShortcut.Save();
            }
            catch (Exception e) {
                throw;
            }
        }

        /*
         * Creates a folder in specified path
         * NOT YET READ ONLY
         */
        public void CreateROFolder(String path) {
            //returns quietly if dir already exists

            if (Directory.Exists(path)) return;

            Directory.CreateDirectory(path);
        }

        public void DeleteFile(String path) {
            if (System.IO.File.Exists(path)) {
                System.IO.File.Delete(path);
            }
        }

        /*
         * deletes an empty folder
         */
        public void DeleteFolder(String path) {
            if (!IsDirectoryEmpty(path))
                return;

            try {
                Directory.Delete(path);
            }
            catch (Exception e) {
                throw e;
            }

        }

        //delete a folder and all its files and sub-folders
        public void DeleteRecursiveFolder(String path) {
            DirectoryInfo parent = new DirectoryInfo(path);
            parent.Delete(true); //recursive delete
        }

        //determine if a directory is empty and eligible for deletion
        public bool IsDirectoryEmpty(string path) {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        /// <summary>
        /// Returns Md5,Path list present in folder
        /// </summary>
        /// <param name="path">The folder from wich we build the results </param>
        /// <returns></returns>
        public List<Tuple<String,String>> GetFicheiroIDSFromFolder(String path) {
            
            List<Tuple<String,String>> md5s = new List<Tuple<String,String>>();
            foreach (String fpath in Directory.EnumerateFiles(path)) {
                try {
                    String md5 = Ficheiro.getFilesMD5Hash(fpath);
                    md5s.Add(new Tuple<string, string>(md5,fpath));
                }
                catch (Exception e) {
                    return null;
                }
            }
            return md5s; 
        }
    }
}

