
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace WeeboxSync {
    public class WeeboxSync {
        private ConnectionInfo _connection_info;
        public ConnectionInfo connection_info {
            get { return _connection_info; }
            set { _connection_info = value; core.SetConnection(value);
                bundle_serial_generator = value.serial_generator;
            }
        }

        public String default_root_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
        public int DefaultSyncInterval { get; set; } //in minutes
        
        private long bundle_serial_generator = 0;
        private List<Scheme> scheme; 
        private  String root_folder = null;
        private CoreAbstraction core;
        private FicheiroSystemAbstraction fileSystem;
        private DataBaseAbstraction dataBase; 
        private String path_schemes= null;
        private String path_bundles= null;

        private readonly object SyncLock = new object ();
        private readonly object BagLock = new object (); //controls access to the collection of to-update bundles
        private List<String> bundlesToUpdate;

        private Watcher watcher;
        private Dictionary<string, DocType> docTypes;

        public WeeboxSync(){
            core = CoreAbstraction.getCore();
            this.docTypes = core.docTypes; 
            fileSystem = new FicheiroSystemAbstraction();
            dataBase = new DataBaseAbstraction(); 
            bundlesToUpdate = new List<string> ();
        }

        public void SetWatcher(ref Watcher watch) {
            watcher = watch;
            watcher.Enable ();
        }
        public bool setCredentials(string user, string pass)
        {
            if (Monitor.TryEnter(SyncLock))
            {
                try
                {
                    connection_info.user = new Utilizador(user, pass);
                    dataBase.SaveConnectionInfo(connection_info);
                }
                catch (Exception e) {
                    
                }
                finally {
                    Monitor.Exit (SyncLock);
                }
                return true;
            }
            return false;
        }
        public bool setConnectionInfo(Uri server, Uri proxy, bool useProxy) {
            if (Monitor.TryEnter(SyncLock))
            {
                try
                {
                    connection_info.address = server;
                    connection_info.proxy = proxy;
                    connection_info.useProxy = useProxy;
                    dataBase.SaveConnectionInfo(connection_info);
                }catch (Exception e) {
                    
                }finally
                {
                    Monitor.Exit(SyncLock);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Adds a bundle to be updated when possible
        /// </summary>
        /// <param name="bundleID">The local ID of the bundle to update</param>
        public void AddBundleToUpdateQueue(string bundleID)
        {
            Monitor.Enter(BagLock);
            try
            {
                //só adicionamos o bundle se ele não estiver à espera de ser sincronizado
                if (!bundlesToUpdate.Contains(bundleID))
                    bundlesToUpdate.Add(bundleID);
            }
            catch (Exception e) {
                
            }
            finally {
                Monitor.Exit (BagLock);
            }

            // tenta sincronizar os bundles
            SyncQueuedBundles ();
        }
        /// <summary>
        /// Attempts to synchronize this weebox instance
        /// </summary>
        /// <returns>true if the instance was synchronized, false if the synchronize process is already ongoing</returns>
        public bool SynchronizeAll() {
            //try to acquire lock and exit if not acquired
            if (Monitor.TryEnter(SyncLock))
            {//lock acquired
                try {
                    watcher.Disable ();
                    syncBundles();
                    watcher.Enable();
                }
                catch (Exception e) {
                    
                }
                finally {
                    Monitor.Exit (SyncLock);
                }
                //tenta sincronizar bundles atrasados
                SyncQueuedBundles ();
            }
            else {
                //acquire failed
                return false;
            }
            return true;
        }
        private bool SyncQueuedBundles()
        {
            if (Monitor.TryEnter(SyncLock))
            {
                try
                {
                    Monitor.Enter(BagLock);
                    try
                    {
                        watcher.Disable();
                        foreach (var bundleID in bundlesToUpdate)
                        {
                            syncBundle(bundleID);
                        }
                        watcher.Enable();
                    }
                    catch (Exception e)
                    {

                    }
                    finally
                    {
                        Monitor.Exit(BagLock);
                    }
                }
                catch (Exception e) {

                } finally {
                    Monitor.Exit (SyncLock);
                }
                return true;
            } else {
                return false;
            }
        }
        public String getRootFolder(){
            return root_folder; 
        }

        public void saveRootFolder(string root) {
            root_folder = root;
            path_schemes = root + "\\Schemes";
            path_bundles = root + "\\bundles";
        }

        /// <summary>
        /// Sets and creates the root folders of this weebox instance
        /// </summary>
        /// a handfull of exceptions
        /// <param name="s">The path where to create the root folders</param>
        /// <returns></returns>
        public bool setRootFolder(string s){
            string path = s + "\\My Weebox";
            string path_Schemes = path + "\\Schemes";
            string path_bundles = path + "\\Bundles"; 
            if (!Directory.Exists(path)){
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path_Schemes);
                Directory.CreateDirectory(path_bundles);

                this.root_folder = path;
                this.path_schemes = path_Schemes;
                this.path_bundles = path_bundles;
                return true;
            }
            else {
                this.root_folder = "";
                this.path_schemes = "";
                this.path_bundles = "";
                return false;     
            }
        }

        public void setDefaultRootFolder(){
            this.setRootFolder(this.default_root_folder); 
        }
        /// <summary>
        /// Downloads all the schemes and bundles in the server. All info needed for the process should be set
        /// </summary>
        public void setup(){
                core.SetConnection(this.connection_info);
                List<Scheme> schemes = core.getSchemesFromServer();
                this.scheme = schemes;

                foreach (Scheme sch in schemes) {
                    Tag root = sch.arvore.getRoot();
                    Directory.CreateDirectory(this.path_schemes + "\\" + root.Path);
                    if (root != null) {
                        _createFolderForTag(root, sch);
                    }
                }

                // save Schemes in bd 
                IEnumerable<String> bundles = core.GetAllBundlesList();
             //   dataBase.SaveClassificationScheme(this.scheme);
                foreach (String weeId in bundles) {
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

         public  void syncScheme(){
             List<Scheme> newScheme = core.getSchemesFromServer();
             foreach (Scheme s in newScheme){
                 foreach (Tag t in s.arvore.getAllValue()){
                     if (Scheme.containsLocal(t.Path, this.scheme))
                         try{
                             fileSystem.CreateROFolder(t.Path);
                         }
                         catch (Exception e){
                             Console.Error.WriteLine("Abruptly terminate program");
                             Console.Read();
                         }
                 }
             }

             foreach (Scheme s in scheme){
                 foreach (Tag t in s.arvore.getAllValue()){
                     if (!Scheme.containsLocal(t.Path, newScheme)){
                         //local tag has been removed 
                         if (Directory.Exists(t.Path)){
                             try{
                                 fileSystem.DeleteRecursiveFolder(t.Path);
                             }
                             catch (Exception e){
                                 Console.WriteLine("Abruptly terminate program");
                             }
                         }
                     }
                 }
             }

             //update classification scheme in bd 
             scheme = newScheme;
             dataBase.SaveClassificationScheme(scheme);
         }

        private void syncBundles(){
             // sync bundles 
             List<String> allBundlesInServer = core.GetAllBundlesList();
             //get all bundles in bd 
             List<Bundle> allBundlesInFS = dataBase.GetAllBundles(); 

             foreach (Bundle bundle in allBundlesInFS){
                 String lastest_version = syncBundle(bundle);
                 if (lastest_version != null){
                     allBundlesInServer.Remove(lastest_version);
                 }
             }
             foreach(String id in allBundlesInServer){
                 CreateBundle(id); 
             }
         }
        /// <summary>
        /// Synchronize one bundle
        /// </summary>
        /// <param name="bundleId">The LOCAL ID of the bundle to be synchronized</param>
        /// <returns></returns>
        public string syncBundle(String bundleId) {
            return syncBundle (dataBase.GetBundle (bundleId));
        }
        /// <summary>
        /// Synchronize one bundle
        /// </summary>
        /// <param name="bundle">The bundle to be synchronized</param>
        /// <returns></returns>
        public string syncBundle(Bundle bundle){
            String bundle_lastest_version_id = core.GetLatestVersionIdFromServer(bundle.weeId);
            if (bundle_lastest_version_id == null){
                //bundle foi eliminado do servidor
                //delete bundle from db 
                //delete bundle from FS
                try{
                    fileSystem.DeleteRecursiveFolder(bundle.getPath(path_bundles));
                    dataBase.DeleteBundle(bundle.localId);
                    foreach (String t in bundle.weeTags){
                        Tag tag;
                        if ((tag = Scheme.getTagByWeeIds(t, scheme)) != null){
                            fileSystem.DeleteFile(tag.Path + "\\" + bundle.localId + ".lnk"); //tags exists 
                        }
                        //else .... -> if the tags have been removed we don't care.
                    }
                return null;

                }catch(Exception e){
                    ; 
                }

                }
                if (bundle_lastest_version_id == bundle.weeId){
                    //Bundle doesn't have a new version on server
                    _sync_with_no_new_version(bundle);
                    Bundle bundle2 = core.GetLatestVersionFromServer (bundle_lastest_version_id);
                    if (bundle2 != null)
                        bundle2.localId = bundle.localId;
                        generateBundleWebPage(bundle2);
                return bundle_lastest_version_id;
            }
            else{
                //bundle has a new version on server
                _sync_with_new_version(bundle);
                Bundle bundle2 = core.GetLatestVersionFromServer(bundle_lastest_version_id);
                if (bundle2 != null) {
                    bundle2.localId = bundle.localId;
                    generateBundleWebPage (bundle2);
                }
                return bundle_lastest_version_id;
            }

        }

        private bool _sync_with_new_version(Bundle bundle){
            //try catch 
            List<Ficheiro> filesFS;
            Bundle bInfo;
            List<Ficheiro> filesCore; 
            List<Ficheiro> filesSync; 
            try{
                 filesFS = fileSystem.getFicheirosFromFolder(bundle.getPath(path_bundles), bundle.localId);
                 bInfo = core.GetLatestVersionFromServer(bundle.weeId);
                if (bInfo == null) return false; // WE SHOULD NOT EVEN BE HERE , DOESNT HAS A NEW VERSION ON SERVER. 
                 filesCore = bInfo.filesPath;
                 filesSync = dataBase.GetFicheirosIDS(bundle.localId);
            }catch(Exception e){
                Console.Error.WriteLine(e);
                return false; 
            }

            bool flag = true; // Indica que posso guardar sincronização com bundle mais recente. 
            String oldBundleVersion = bundle.weeId; 
            String transformedBundleId = bInfo.weeId;
            // where whe apply the modifications and save the new bundle wee id.

            //Alterar ficheiros que APENAS mudaram de nome 
            transformedBundleId = _alterFileNames(ref bundle, ref filesFS, ref filesCore, ref filesSync,
                                                  transformedBundleId, ref flag);


            List<Ficheiro> porTratarFS = new List<Ficheiro>(filesFS);
            // updates nos dois lados , fica com os dois TODO

            //Se para um ficheiro no filesystem com um nome x
            // não existir uma ultima versão sincronizada ou a última versão sincronizada tem um id diferente no core( houve update também no core)
            // Cria-se uma cópia duplicada. 
            foreach (Ficheiro fileSystemFile in filesFS){
                Ficheiro fileSynced = filesSync.Find((Ficheiro x) => x.md5 == fileSystemFile.md5);
                Ficheiro fileCored = filesCore.Find((Ficheiro x) => x.md5 == fileSystemFile.md5);
                bool duplicate_copy = false;

                if (fileSynced == null && fileCored == null){
                    // Id do ficheiro só existe no fileSystem! 

                    //Verificar se existem ficheiros com o mesmo nome. 
                    Ficheiro fileSyncedSameName = filesSync.Find((Ficheiro x) => x.name == fileSystemFile.name);
                    Ficheiro fileCoredSameName = filesCore.Find((Ficheiro x) => x.name == fileSystemFile.name);

                    if (fileCoredSameName != null){
                        //Existe um ficheiro no core com o mesmo nome 
                        //Vamos verificar : 
                        // Se existir um ficheiro com esse nome sincronizado e tiver um id diferente do que existe no core( ficheiro levou update dos dois lados) 
                        // Ou então não há informação de sincronização e o ficheiro também foi updated dos dois lados . 
                        // Em ambos casos considera-se uma cópia duplicada. 
                        if (fileSyncedSameName != null){
                            if (fileSyncedSameName.md5 != fileCoredSameName.md5){
                                duplicate_copy = true;
                            }
                        }
                        else{
                            duplicate_copy = true;
                        }
                    }
                    // if duplicate copy is true , we must treat the exception. 
                    // modificar o nome do ficheiro, fazer update no core,  sincronizar e retirar caso das listas. 
                    if (duplicate_copy){
                        String newPath = fileSystem.getDuplicateName(fileSystemFile.path);
                        String newName = fileSystem.getName(newPath); 

                        String oldNAme = fileSystemFile.name;
                        String oldPath = fileSystemFile.path; 
                        fileSystemFile.name = newName ;
                        try{
                            fileSystem.RenameFile(oldPath, newPath);
                            fileSystemFile.path = newPath; 
                            transformedBundleId = core.PutFicheiro(transformedBundleId, fileSystemFile);
                            dataBase.DeleteFicheiroInfo(fileSyncedSameName.md5, bundle.localId);
                            dataBase.SaveFicheiroInfo(fileSystemFile);
                            dataBase.UpdateFicheiroInfo(fileSystemFile);
                            dataBase.UpdateWeeId(bundle.localId, transformedBundleId);
                            String newFilePath = bundle.getPath(root_folder) + "\\" + fileCoredSameName.name; 
                            core.GetFicheiro(transformedBundleId, fileCoredSameName.md5,
                                             newFilePath); 
                            Ficheiro toSaveInBD = new Ficheiro(newFilePath, bundle.localId, fileCoredSameName.md5);
                            dataBase.SaveFicheiroInfo(toSaveInBD);
                        }catch(Exception e){
                            //Adia tratamento dos ficheiros envolvidos para depois. 
                            //Removendo ficheiros envolvidos das listas a tratar neste método. 
                            porTratarFS.RemoveAll((Ficheiro x) => x.name == oldNAme);
                            filesCore.Remove(fileCoredSameName); 
                            if (fileSyncedSameName != null){
                                filesSync.Remove(fileSyncedSameName); 
                            }
                            //Se tivermos conseguido mudar o nome do ficheiro acima , então tenta renomear para o nome original.
                            try{
                                if (File.Exists(newPath)){
                                    fileSystem.RenameFile(newPath, oldPath);                                    
                                }
                            }catch(Exception e1){
                                // Ignorar exception , best effort rename file, não funciona , continua 
                                //Teremos um _DuplicateCopy_File que será considerado na próxima iteração. 
                                ; 
                            }
                            flag = false; 
                            continue; 
                        } 
                        filesCore.RemoveAll((Ficheiro x) => x.name == fileSystemFile.name);
                        filesSync.RemoveAll((Ficheiro x) => x.name == fileSystemFile.name);
                        porTratarFS.Remove(fileSystemFile);
                    }
                }
            }
            filesFS = porTratarFS;

            porTratarFS = new List<Ficheiro>(filesFS);
            foreach (Ficheiro fileSystemFile in filesFS){
                Ficheiro fileSynced = filesSync.Find((Ficheiro x) => x.md5 == fileSystemFile.md5);
                Ficheiro fileCored = filesCore.Find((Ficheiro x) => x.md5 == fileSystemFile.md5);

                // Ficheiros só presentes no file system 
                if (fileSynced == null && fileCored == null){
                    //novo ficheiro no fs. 
                    Ficheiro coreSameNameFile = filesCore.Find((Ficheiro x) => x.name == fileSystemFile.name);
                    Ficheiro bdSameNameFile = filesSync.Find((Ficheiro x) => x.name == fileSystemFile.name);

                    try{
                        // Se existir um ficheiro no core com o mesmo nome , este deve ser removido, pois é uma versão antiga. 
                        if (coreSameNameFile != null){
                            Tuple<String, List<String>> removedInfo = core.RemoveFicheiro(transformedBundleId,
                                                                                          coreSameNameFile.md5);
                            transformedBundleId = removedInfo.Item1;
                            dataBase.UpdateWeeId(bundle.localId, transformedBundleId);
                        }
                        if (bdSameNameFile != null){
                            dataBase.DeleteFicheiroInfo(bdSameNameFile.md5, bdSameNameFile.bundleId);
                        }

                        //adicionar ficheiro novo ao core
                        transformedBundleId = core.PutFicheiro(transformedBundleId, fileSystemFile);
                        dataBase.UpdateFicheiroInfo(fileSystemFile);
                        dataBase.UpdateWeeId(bundle.localId, transformedBundleId);
                    }catch(Exception e){
                        flag = false; 
                        continue;
                    }
                    finally{
                        //apagar md5 da lista actual , nome das outras
                        porTratarFS.Remove(fileSystemFile);
                        filesCore.Remove(coreSameNameFile);
                        filesSync.Remove(bdSameNameFile); 
                    }
                }

                // id do ficheiro só nao existe no core
                //Preciso de o apagar do file system , e da base de dados.
                if ((fileSynced != null) && (fileCored == null)){
                    try{
                        fileSystem.DeleteFile(fileSystemFile.path);
                        dataBase.DeleteFicheiroInfo(fileSynced.md5, fileSynced.bundleId);
                    }
                    catch (Exception e){
                        flag = false; 
                        continue;
                    }
                    finally{
                        porTratarFS.Remove(fileSystemFile);
                        filesSync.Remove(fileSynced);
                    }
                }
            }

            filesFS = porTratarFS; 

            List<Ficheiro> porTratarCored = new List<Ficheiro>(filesCore);

            foreach (Ficheiro fileCored in filesCore){

                Ficheiro fileFS = filesFS.Find((Ficheiro x) => x.md5 == fileCored.md5);
                Ficheiro fileSync = filesSync.Find((Ficheiro x) => x.md5 == fileCored.md5);

                if ((fileFS == null) && (fileSync == null)){
                    //File só  existe no bundle do core

                    Ficheiro fileSameNameFS = filesFS.Find((Ficheiro x) => x.name == fileCored.name);
                    Ficheiro fileSameNameSynced = filesSync.Find((Ficheiro x) => x.name == fileCored.name);
                    string file_path = bundle.getPath(path_bundles) + "\\" + fileCored.name;

                    try{
                        if (fileSameNameFS != null){
                            // Fizeste update no core e queres sacar para o file system.
                            //file already exits. md5 differs, 
                            //try remove it or change is name.
                            fileSystem.DeleteFile(file_path);
                            dataBase.DeleteFicheiroInfo(fileSameNameFS.md5, bundle.localId);
                            core.GetFicheiro(transformedBundleId, fileCored.md5, file_path);
                        }
                        else{
                            core.GetFicheiro(transformedBundleId, fileCored.md5, file_path);
                        }
                    }
                    catch{
                        continue;
                    }
                    finally{
                        porTratarFS.Remove(fileSameNameFS);
                        filesSync.Remove(fileSameNameSynced);
                        porTratarCored.Remove(fileCored);
                    }
                }

                else if ((fileSync != null) && (fileFS == null)){
                    //ficheiro existe na base de dados e existe no core ( Só nao existe no filesystem , tendo sido removido (provavelmente))
                    try{
                        core.RemoveFicheiro(transformedBundleId, fileCored.md5);
                        dataBase.DeleteFicheiroInfo(fileSync.md5, bundle.localId);
                    }
                    catch (Exception e){
                        flag = false; 
                        continue;
                    }
                    finally{
                        porTratarCored.Remove(fileCored);
                        filesSync.Remove(fileSync);
                    }
                }
            }
            filesCore = porTratarCored; 

            //remove md5 from core list and remove same name from every list

            foreach (Ficheiro fileSynced in filesSync){
                dataBase.DeleteFicheiroInfo(fileSynced.md5, bundle.localId);
            }
            if (flag ){
                try{
                    dataBase.UpdateWeeId(bundle.localId, transformedBundleId);
                }catch(Exception e){
                    flag = false; 
                }
            }
            else{
                // Foram encontrados erros durante sincronização. Desta forma força-se a que na próxima vez seja sincronizado por o has new version  
                dataBase.UpdateWeeId(bundle.localId, oldBundleVersion);
            }
            //TODO erase all updateweeIds above. 
            return true; 
        }


        private
            String _alterFileNames
            (ref Bundle bundle, ref List<Ficheiro> filesFS, ref List<Ficheiro> filesCore, ref List<Ficheiro> filesSync,
             string bundleId,ref bool flag )
            {
                List<Ficheiro> copyFilesCore = filesCore;
                String transformedBundleID = bundleId;
                List<Ficheiro> newNameFiles =
                    filesFS.FindAll(
                        (Ficheiro fileFS) =>
                        copyFilesCore.Exists(
                            (Ficheiro coreFile) => coreFile.md5 == fileFS.md5 && coreFile.name != fileFS.name));

                foreach (Ficheiro newNameFile in newNameFiles){
                    //ver quem é que mudou de nome 
                    Ficheiro inFileSys = newNameFile;
                    Ficheiro inSyncedFile = filesSync.Find((Ficheiro x) => x.md5 == inFileSys.md5);
                    Ficheiro inCoreFile = filesCore.Find((Ficheiro x) => x.md5 == inFileSys.md5);
                    if ((inSyncedFile == null) ||
                        ((inSyncedFile.name != inCoreFile.name) && inSyncedFile.name != inFileSys.name)){
                        // files wasn't synced , we don't have a way to find out who was the last one to be renamed.  OR  file changed in both places 
                        // policy : stay with server name , faster solution 

                        Ficheiro newFile = new Ficheiro(bundle.getPath(path_bundles) + "\\" + inCoreFile.name,
                                                        bundle.localId, inCoreFile.md5);
                        try{
                            fileSystem.RenameFile(inFileSys.path, newFile.path);
                            dataBase.DeleteFicheiroInfo(inFileSys.md5, bundle.localId);
                            dataBase.SaveFicheiroInfo(newFile);
                        }
                        catch (Exception e){
                            // We could not rename the file 
                            //ignoring file
                            flag = false; 
                            continue;
                        }
                    }
                    else if (((inSyncedFile != null) && (inSyncedFile.name == inCoreFile.name))){
                            //mudou de nome só localmente. 
                            try{
                                //TODO save changes to database
                                Tuple<String, List<String>> removedInfo = core.RemoveFicheiro(bundle.weeId,
                                                                                              newNameFile.md5);
                                if (removedInfo.Item2.Count != 1){
                                    throw new SystemException("Could not update, being catched right below");
                                }
                                transformedBundleID = removedInfo.Item1;
                                dataBase.DeleteFicheiroInfo(inSyncedFile.md5, bundle.localId);
                                transformedBundleID = core.PutFicheiro(transformedBundleID, newNameFile);
                                dataBase.SaveFicheiroInfo(newNameFile);
                                // might throw another exception being catch below
                            }
                            catch (Exception e){
                                //we could not rename the file continue with sync; 
                                flag = false; 
                                continue; //outer loop , syncing other files. 
                            }
                        }
                        else{
                            //file changed remotly ; rename local file. 
                            Ficheiro newFile = new Ficheiro(bundle.getPath(path_bundles) + "\\" + inCoreFile.name,
                                                            bundle.localId, inFileSys.md5);
                            try{
                                fileSystem.RenameFile(inFileSys.path, newFile.path);
                                dataBase.UpdateFicheiroInfo(newFile);
                            }
                            catch (Exception e){
                                flag = false; 
                                // We could not rename the file 
                                //ignoring file 
                                continue;
                            }
                        }
                    }

                return transformedBundleID;
            }


        public static String webPage1 =
            @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\""> <html xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""en"" lang=""en"">    <head>        <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />        <title>Weebox-Sync Bundle Metadata</title>        <link rel=""stylesheet"" type=""text/css"" href=""styles.css"" />""         
</head>    <body>    	<div class=""section"" id=""page"">             <div class=""header"">  
                        <h3>Weebox Metadata</h3>            </div>
            <div class=""section"" id=""articles""> 

                <div class=""article"" id=""article1"">   <div class=""line""></div> ";

        public static String foot =
            @"
                    </div>
                </div>
            </div>
        <div class=""footer""> 
          <div class=""line""></div>
           <p>Weebox Sync Powered</p> 
           <a href=""#"" class=""up"">Go UP</a>
           <a href=""http://www.uminho.pt/"" class=""by"">Grupo 10</a>
        </div>
		</div> 
        <!-- JavaScript Includes -->
        <script type=""text/javascript"" src=""http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js""></script>
        <script type=""text/javascript"" src=""jquery.scrollTo-1.4.2/jquery.scrollTo-min.js""></script>
        <script type=""text/javascript"" src=""script.js""></script>
    </body>
</html>"; 
        public void generateBundleWebPage(Bundle b){
            string filePath = "\\fold\\" + b.weeId + ".html"; 
            try{
                FileStream st =new FileStream(filePath, FileMode.Create); 
                using (StreamWriter  streamOut = new StreamWriter(st)){              
                    streamOut.WriteLine(webPage1);
                    streamOut.WriteLine("<h2>" +  b.type.label+ " </h2> <div class=\"articleBody clear>\"");

                    foreach (DocType.Field f in b.type.fields ){
                        if (b.meta.keyValueData.ContainsKey(f.id)){
                            string s = b.meta.keyValueData[f.id]; 
                            if (s!= "" && f.type != "vocabulary"){
                                streamOut.WriteLine("<span title=\"" + f.description + "\">" + "<h4><b>" + f.label  + "</b></h4></span>");
                                streamOut.WriteLine("<p>" + s + "</p>");
                                streamOut.WriteLine(@"<div class=""line""></div>");
                            }
                        }
                    }
                    streamOut.WriteLine(foot);
                    if (File.Exists(b.getPath(path_bundles) + "\\" + "View Metadata")){
                        File.Delete(b.getPath(path_bundles) + "\\" + "View Metadata");
                    }
                    fileSystem.CreateROLink(b.getPath(path_bundles), filePath, "View Metadata");
                }
            }catch(Exception e ){
                return; 
            }

        }
        
        private bool _sync_with_no_new_version(Bundle bundle){
            List<Ficheiro> filesFs;
            List<Ficheiro> filesLastSync; 
            try{
                filesFs = fileSystem.getFicheirosFromFolder(bundle.getPath(path_bundles), bundle.localId);
                filesLastSync = dataBase.GetFicheirosIDS(bundle.localId);
            }catch(Exception e ){
                return false; 
            }

            //TODO - speed things up , create a list foreach case.
            if (filesFs==null || filesLastSync == null ){
                return false; 
            }

            String bundleTransformedId = bundle.weeId;
            //ficheiros que mudaram de nome 
            //if files exists in both bundles but has different name 
            List<Ficheiro> newNameFiles =
                filesFs.FindAll(
                    (Ficheiro fsFile) =>
                    filesLastSync.Exists(
                        (Ficheiro syncFile) => (syncFile.md5 == fsFile.md5) && (syncFile.name != fsFile.name)));
            foreach (Ficheiro newNameFile in newNameFiles){
                //eliminar e adicionar ficheiro é a unica forma de renomear o ficheiro
                Ficheiro syncFile = filesLastSync.Find((Ficheiro x) => x.md5 == newNameFile.md5);
                try{
                    Tuple<String, List<String>> removedInfo = core.RemoveFicheiro(bundle.weeId, newNameFile.md5);
                    bundleTransformedId = removedInfo.Item1;
                    dataBase.UpdateWeeId(bundle.localId, bundleTransformedId);
                    if (removedInfo.Item2.Count == 0){
                        dataBase.DeleteFicheiroInfo(newNameFile.md5, bundle.localId);
                        bundleTransformedId = core.PutFicheiro(bundleTransformedId, newNameFile);
                        dataBase.UpdateWeeId(bundle.localId, bundleTransformedId);
                        dataBase.SaveFicheiroInfo(newNameFile);
                    }
                }catch(Exception e){
                    Console.WriteLine(e.Message);
                    continue; 
                }
                }


            // ficheiros só no file system
            foreach (Ficheiro file in filesFs){
                //if file only exists in file system 
                if (!filesLastSync.Exists((Ficheiro syncFile) => syncFile.md5 == file.md5)){
                    try{
                        Ficheiro fileCored = filesLastSync.Find((Ficheiro x) => x.name == file.name); 
                        // if fileCored exits it has the same name , we have to removed beacause it's an update of the file
                        // se calhar devias verificar no core primeiro :S
                        if (fileCored != null) {
                            Tuple<String, List<String>> removedInfo = core.RemoveFicheiro (bundleTransformedId,
                                                                                           fileCored.md5);
                            bundleTransformedId = removedInfo.Item1;
                            dataBase.UpdateWeeId (bundle.localId, bundleTransformedId);
                            if (removedInfo.Item2.Count == 0) {
                                dataBase.DeleteFicheiroInfo (fileCored.md5, fileCored.bundleId);
                            }
                        }
                        bundleTransformedId = core.PutFicheiro (bundleTransformedId, file);
                        dataBase.UpdateWeeId (bundle.localId, bundleTransformedId);
                        dataBase.SaveFicheiroInfo (file);
                    }catch(Exception e){
                        Console.WriteLine(e);
                        continue; 
                    }
                }
            }

            foreach (Ficheiro file in filesLastSync){
                // file only exists in last synced version
                if (!filesFs.Exists((Ficheiro fileFs) => fileFs.md5 == file.md5)){
                    try{
                        Tuple<String, List<String>> removedInfo = core.RemoveFicheiro(bundleTransformedId, file.md5);
                        bundleTransformedId = removedInfo.Item1;
                        if (removedInfo.Item2.Count == 0){
                            dataBase.DeleteFicheiroInfo(file.md5, file.bundleId);
                        }
                        dataBase.UpdateWeeId(bundle.localId, bundleTransformedId);
                    }catch(Exception e){
                        Console.WriteLine(e);
                        continue; 
                    }
                }
            }
            return true;
        }

        public bool CreateBundle(String bundleId_server_id){
            //deve garantir que os planos de classificação estão actualizados 
            string path_bundle = this.path_bundles + "\\" + this.bundle_serial_generator;
            Directory.CreateDirectory(path_bundle); 
            Bundle b = core.getBundle(bundleId_server_id, path_bundle);
            if (b == null) return false; 
            b.localId = "" + this.bundle_serial_generator;
            this.bundle_serial_generator += 1; 
            if (b.weeTags != null){
                foreach (String t in b.weeTags){
                    Tag tag = Scheme.getTagByWeeIds(t, this.scheme);
                    if (tag != null){
                        //                   fileSystem.CreateROLink(path_bundle, tag.Path);
                        fileSystem.CreateROLink(this.path_schemes + "\\" + tag.Path, path_bundle + "\\", b.localId);
                    }
                }
            }
            dataBase.SaveBundle(b);
            generateBundleWebPage(b);
            return true; //has created bundle
        }
                public void GetNewBundles() {
            throw new System.Exception("Not implemented");
        }

        public void TestaConexao(object server, object porta, object proxy) {
            throw new System.Exception("Not implemented");
        }

    }


}