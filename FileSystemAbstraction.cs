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
        /// Returns files in folder. 
        /// </summary>
        /// <param name="path">The folder from wich we build the results </param>
        /// <returns></returns>

        public  string getName(string path ){
            if (!path.Contains("\\")){
                return path;
            }
            String s = "" + path;
            int lastInfexOF   = s.LastIndexOf("\\");
            return s.Remove(0, lastInfexOF+1 ); 

        }
        public  String getDuplicateName(string path){
            String name=path; 
            int lastInfexOF   = name.LastIndexOf("\\");
            name = name.Remove(0, lastInfexOF+1 );
            String realPath = path.Remove(lastInfexOF);
            String ext = "";
            int i = 0;
            while (System.IO.File.Exists(realPath + i + name)){
                i++; 
            }
            return realPath + i + name; 
        }

        public List<Ficheiro> getFicheirosFromFolder(String path, string bundleId){
            List<Ficheiro> files = new List<Ficheiro>();
            foreach (String fpath in Directory.EnumerateFiles(path)) {
                try
                {
                    if ((System.IO.File.GetAttributes(fpath) & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden)
                    {
                        if (getName(fpath) != "View Metadata.lnk") {
                        Ficheiro file = new Ficheiro(fpath, bundleId, true);
                        files.Add(file);
                    }
                    }
                }
                catch (Exception e){
                    try{
                        String tempPath = System.IO.Path.GetTempPath();
                        String filePath = tempPath + System.IO.Path.GetFileName(fpath); 
                        System.IO.File.Copy(fpath, filePath, true);
                        files.Add(new Ficheiro(filePath, bundleId,true));
                    }
                    catch (IOException e2){
                        return null; 
                    }
                }
            }
            return files; 
        }

        public void RenameFile(string path , string newpath){
            if (System.IO.File.Exists(path)){
                System.IO.File.Move(path, newpath);
            }
        }
    }
}

