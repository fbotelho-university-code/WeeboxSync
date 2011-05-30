using System;
namespace WeeboxSync {
    using System.IO;

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
        }

        public void Enable() {
            watcher.EnableRaisingEvents = true;
        }
        public void Disable() {
            watcher.EnableRaisingEvents = true;
        }
        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e) {
            string rootPath = weebox.getRootFolder();
            if (e.FullPath.Contains(rootPath)) {
                string bundleID = e.FullPath.Substring (rootPath.Length); 
                weebox.AddBundleToUpdateQueue(bundleID);
            }
        }
    }
}