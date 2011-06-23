using System;
namespace WeeboxSync {
    using System.IO;
    using System.Windows.Forms;

    public class Watcher {
        private string path;
        private FileSystemWatcher watcher;
        private WeeboxSync weebox;

        public Watcher(ref WeeboxSync weebox, string path) {
            this.path = path;
            this.weebox = weebox;

            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher {
                                                Path = path,
                                                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Attributes | NotifyFilters.Size
                                            };
            // Add event handlers.
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnChanged;
            
            watcher.IncludeSubdirectories = true;
            Enable ();
        }

        public void Enable() {
            watcher.EnableRaisingEvents = true;
        }
        public void Disable() {
            watcher.EnableRaisingEvents = false;
        }
        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e) {
            
            if (e.FullPath.Contains(path)) {
                string bundleID = e.FullPath.Substring (path.Length+1);
                bundleID = bundleID.Substring(0, bundleID.IndexOf ("\\"));
                //MessageBox.Show ("FullPath:\n" + e.FullPath + "\nBundleID:\n" + bundleID + "\nPath:\n" + path);
                weebox.AddBundleToUpdateQueue(bundleID);
            }
        }
    }
}