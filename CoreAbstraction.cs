using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.Http;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using Ionic.Zip;
using Microsoft.Http.Headers;
using System.IO;
using Microsoft.Http.Headers;

namespace WeeboxSync {
    public class ConnectionNotSetException : Exception {

    }

    public class AccessDeniedException : Exception {

    }

    public class ServerErrorException : Exception {

    }
    
    public class RequestTimeOutException : Exception{
        
    }


    public class HttpError : ApplicationException{
        public  HttpStatusCode statusCode; 
        public string context; 
        public HttpError(HttpStatusCode statusCode, string context){
            this.statusCode = statusCode;
            this.context = context;
        }
    }


    public class CoreAbstraction {
        private static CoreAbstraction core = null;
        private CoreAbstraction() { }
        public Dictionary<String, DocType> docTypes = new Dictionary<string, DocType>();
        /// <summary>
        /// Instantiate private Core Abstraction 
        /// </summary>
        /// <returns>The only CoreAbstraction instance present</returns>
        public static CoreAbstraction getCore() { if (core == null) return new CoreAbstraction(); else return core; }
        private HttpClient _client;
        private ConnectionInfo _conInfo;
        private bool _connection = false;

        /// <summary>
        /// Sets the connection information to contact the server. 
        /// </summary>
        /// <param name="con"> The connection information</param>
        public void SetConnection(ConnectionInfo con) {
            _conInfo = con;
            if (con.address != null) {
                _client = new HttpClient(con.address.ToString());
            }
            if (con.user != null) {
                _client.TransportSettings.Credentials = new NetworkCredential(con.user.user, con.user.pass);
            }
            if (con.useProxy && con.proxy != null) {
                _client.TransportSettings.Proxy = new WebProxy(con.proxy);
            }
            _client.TransportSettings.PreAuthenticate = true;
        }

        /**
         * Returns null if no scheme is returned
         */
        public List<Scheme> getSchemesFromServer() {
            //TODO - ALOT , exceptions , null values , empty values, server errors , validate xml input in these and look a like methods

            HttpResponseMessage resp = null;
            bool bo = true;
            while (bo) {
                try {
                    resp = _client.Get("manager/api?operation=getThesauri");
                    resp.EnsureStatusIsSuccessful(); 
                    bo = false;
                }
                catch (HttpStageProcessingException e) {
                    continue;
                }
            }

                       _checkAndThrowsExceptions(resp.StatusCode, "getSchemesFromServer");
            String schemes = resp.Content.ReadAsString();
            String[] planos = schemes.Split('\n');
            List<Scheme> lista = new List<Scheme>();
            for (int i = 0; i < planos.Length; i++) {
                if (planos[i] != "") {
                    Scheme s = getScheme(planos[i]);
                    if (s != null) { // TODO - necessary? 
                        lista.Add(s);
                    }
                }
            }
            return (lista.Count != 0) ? lista : null;
        }

        private Scheme getScheme(string rootID) {
            //TODO - check every , see getSchemes comments
            HttpResponseMessage resp = null;
            bool bo = true;
            while (bo) {
                try {
                    resp = _client.Get("manager/api?operation=getThesaurus&thesaurus=" + rootID);
                    bo = false;
                }
                catch (HttpStageProcessingException e) {
                    continue;
                }

            }
            _checkAndThrowsExceptions(resp.StatusCode, "getSchemeFromServer");
            Stream s = resp.Content.ReadAsStream();
            // TODO - validate xml answer. 
            XDocument classifications = XDocument.Load(s);
            XElement rootElement = classifications.Root;

            //TODO - check to see if this will return always the value we want for the root. 
            XElement raiz = rootElement.Element(_getSkoName("ConceptScheme"));

            Tag rootTag = new Tag(raiz.Value, raiz.Value, "Q2W1C42bT6");
            Scheme scheme = new Scheme("Q2W1C42bT6", rootTag);
            //<id,label> 
            IEnumerable<Tuple<string, string>> lista = _getFirstLevelChilds(rootElement);

            foreach (Tuple<String, String> tup in lista) {
                string myPath = rootTag.Path + "\\" + tup.Item2;
                Tag t = new Tag(tup.Item2, myPath, tup.Item1);
                scheme.arvore.add(t, rootTag.Path, t.Path);
                scheme.arvoreByWeeboxIds.add(t, rootTag.WeeId, t.WeeId);
                _buildSubTree(rootElement, t, scheme);
            }

            return scheme;
        }

        public List<Bundle> GetAllBundles() {
            throw new System.Exception("Not implemented");
        }

        /// <summary>
        /// Gets all Bundles specified as owned by the user established in this class
        /// </summary>
        /// <returns>Null if none present</returns>
        public List<String> GetAllBundlesList(){
            HttpResponseMessage resp = null;
            bool bo = true;
            while (bo) {
                try {
                    resp = _client.Get("core/bundle/?operation=searchRetrieve&version=1.1&query=bundle.owner+=+%22" +
                            this._conInfo.user.user + "%22");

                    _checkAndThrowsExceptions(resp.StatusCode, "GetAllBundlesList");
                    bo = false;
                }
                catch (HttpStageProcessingException e) {
                    continue;
                }
            }

            Stream s = resp.Content.ReadAsStream();
            // TODO - validate xml answer. 
            XDocument classifications = XDocument.Load(s);
            XElement rootElement = classifications.Root;

            List<String> lista = new List<string>();
            foreach (XElement record in rootElement.Descendants(_getLocName("recordData"))) {
                String ss = record.Value;
                string pattern = "(<entry key=\"bundle.id\">)(\\w*)(</entry>)";
                MatchCollection matches = Regex.Matches(ss, pattern);
                foreach (Match matche in matches) {
                    if (matche.Groups.Count >= 1) {
                        Group group = matche.Groups[2];
                        lista.Add(group.Value);
                    }
                }
            }
            if (lista.Count >= 0) return lista; else return null;

        }

        public MetaData GetMetaFromBundle(String bundleid) {
            try {

                HttpResponseMessage resp = null;
                bool bo = true;
                while (bo) {
                    try {
                        resp =
                            _client.Get("core/bundle/" + bundleid + "?operation=retrieveBundleMetadata");
                                            _checkAndThrowsExceptions(resp.StatusCode, "GetAllBundlesList");
                        bo = false;
                    }
                    catch (HttpStageProcessingException e) {
                        continue;
                    }
                }

                Stream s = resp.Content.ReadAsStream();
                XDocument meta = XDocument.Load(s);
                XElement rootElement = meta.Root;

                MetaData metaData = new MetaData();
                foreach (XElement e in rootElement.Descendants()) {
                    //TODO - clean up the house, maybe read the attributes one by one into real values and types
                    if (!e.Value.Equals("")) metaData.keyValueData.Add(e.FirstAttribute.Value, e.Value);

                }
                return metaData;
            }
            catch (ArgumentOutOfRangeException) {
                return null;
            }
        }

        public MetaData GetAllMetaFromBundle(String bundleid) {
            try {
                HttpResponseMessage resp = null;
                bool bo = true;
                while (bo) {
                    try {
                        resp = _client.Get("core/bundle/" + bundleid + "?operation=retrieveAllMetadata");

                        _checkAndThrowsExceptions(resp.StatusCode, "GetAllMetaFromBundle");
                        bo = false;
                    }
                    catch (HttpStageProcessingException e) {
                        continue;
                    }
                }

                resp.EnsureStatusIsSuccessful();
                Stream s = resp.Content.ReadAsStream();
                XDocument meta = XDocument.Load(s);
                XElement rootElement = meta.Root;

                MetaData metaData = new MetaData();
                foreach (XElement e in rootElement.Descendants()) {
                    //TODO - clean up the house, maybe read the attributes one by one into real values and types
                    if (!e.Value.Equals("")) metaData.keyValueData.Add(e.FirstAttribute.Value, e.Value);
                }
                return metaData;
            }
            catch (ArgumentOutOfRangeException) {
                return null;
            }
        }

        public Bundle getBundleInfo(string bundleId) {
            Bundle toBeBundle = new Bundle { meta = GetAllMetaFromBundle(bundleId), weeId = bundleId };
            if (toBeBundle == null || toBeBundle.meta == null) return null;

            if (toBeBundle.meta.keyValueData.ContainsKey("dc.type")) {
                String idDocType = toBeBundle.meta.keyValueData["dc.type"];
                if (!this.docTypes.ContainsKey(idDocType)) {
                    //try to get new docTypes
                    this.docTypes = getDocTypes();
                }

                if (!this.docTypes.ContainsKey(idDocType)) {
                    //still don't know the doc type return null 
                    return null;
                }
                toBeBundle.type = this.docTypes[idDocType];
            }
            _parseBundleTags(ref toBeBundle);
            _extracFilesInfo(ref toBeBundle);
            return toBeBundle;
        }

        private void _extracFilesInfo(ref Bundle toBeBundle) {
            string bundleId = toBeBundle.weeId;
            List<String> ids = toBeBundle.meta.getFilesMd5s();
            List<Ficheiro> files = new List<Ficheiro>();
            if (ids != null) {
                foreach (String fid in ids) {
                    HttpResponseMessage resp = null;
                    bool bo = true;
                    if (ids != null) {
                        //has files 
                        while (bo) {
                            try {
                                resp = _client.Get("core/file/" + bundleId + "/" + fid + "?operation=retrieveFileMetadata");
                                                    _checkAndThrowsExceptions(resp.StatusCode, "extract files info");  
                                bo = false;
                            }
                            catch (HttpStageProcessingException e) {
                                continue;
                            }
                        }
                    }


                    //parse file names 
                    String s = resp.Content.ReadAsString();
                    string pattern = "(<entry key=\"file.filename\">)([^<]*)(</entry>)";
                    MatchCollection matches = Regex.Matches(s, pattern);
                    foreach (Match matche in matches) {
                        if (matche.Groups.Count >= 1) {
                            Group group = matche.Groups[2];
                            toBeBundle.filesPath.Add(new Ficheiro(group.Value, bundleId, fid));
                        }
                    }
                }
            }
        }

        private void _parseBundleTags(ref Bundle toBeBundle) {
            List<String> tags = new List<String>();

            foreach (DocType.Field field in toBeBundle.type.tagFields) {
                if (toBeBundle.meta.keyValueData.ContainsKey(field.id)) {
                    string toParse = "";
                    toBeBundle.meta.keyValueData.TryGetValue(field.id, out toParse);
                    String[] strs = Regex.Split(toParse, ";;");
                    foreach (String st in strs) {
                        String[] tagss = Regex.Split(st, "::");
                        tags.Add(tagss.Last<String>().Trim());
                    }
                }
            }
            toBeBundle.weeTags = tags;
        }

        public Bundle getBundle(string bundleId, string downloadPath) {
            Bundle toBeBundle = getBundleInfo(bundleId);
            if (toBeBundle == null) return null;

            HttpResponseMessage resp = null;
            bool bo = true;

            while (bo) {
                try {
                    resp =
                        _client.Get("core/bundle/" + bundleId + "?encodeFileName=true");
                    if (resp.StatusCode != HttpStatusCode.OK)
                        if ((resp.StatusCode == HttpStatusCode.NotFound ) && (toBeBundle.filesPath.Count  == 0  )){
                            
                        }
                        else{
                                            _checkAndThrowsExceptions(resp.StatusCode, "getBundle"); 
                        }

                    bo = false;
                }
                catch (HttpStageProcessingException e) {
                    continue;
                }
            }

            // resp.EnsureStatusIsSuccessful();
            //alter the files path set by getBundleInfo 

            toBeBundle.filesPath.ForEach((Ficheiro x) => x.path = downloadPath + "\\" + x.name);

            //zip case)
            if (toBeBundle.filesPath.Count > 1) {
                Stream files = resp.Content.ReadAsStream();
                string zip_path = downloadPath + "\\" + bundleId + ".zip";
                if (File.Exists(zip_path)) {
                    File.Delete(zip_path);
                }

                FileStream f = File.OpenWrite(zip_path);
                files.CopyTo(f);
                f.Flush();
                f.Close();

                List<Ficheiro> ficheiros = new List<Ficheiro>();

                using (
                    ZipFile zip = ZipFile.Read(zip_path)) {
                    //TODO - what if files exists? 
                    zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                    zip.ExtractAll(downloadPath);
                    ficheiros.AddRange(
                        zip.EntryFileNames.Select(
                            entrada => new Ficheiro(downloadPath + "\\" + entrada, bundleId, true)));
                }
                File.Delete(zip_path);
            }
            if (toBeBundle.filesPath.Count == 1) {
                Ficheiro file = toBeBundle.filesPath.First();
                Stream files = resp.Content.ReadAsStream();
                string file_path = downloadPath + "\\" + file.name;
                if (File.Exists(file_path)) {
                    File.Delete(file_path);
                }
                FileStream f = File.OpenWrite(file_path);
                files.CopyTo(f);
                f.Flush();
                f.Close();
                List<Ficheiro> ficheiros = new List<Ficheiro>();
                ficheiros.Add(new Ficheiro(file_path, bundleId, true));
            }
            return toBeBundle;
        }

        public Dictionary<String, DocType> getDocTypes() {
            HttpResponseMessage resp = _client.Get("manager/api?operation=getDocTypes");
            _checkAndThrowsExceptions(resp.StatusCode, "getDocTypes"); 

            //resp.EnsureStatusIsSuccessful();
            String[] docs = resp.Content.ReadAsString().Split('\n');
            Dictionary<String, DocType> map = new Dictionary<String, DocType>();

            for (int i = 0; i < docs.Length; i++) {
                if (docs[i] != "") {
                    resp = _client.Get("manager/api?operation=getDocType&docType=" + docs[i]);
                    _checkAndThrowsExceptions(resp.StatusCode, "getting doc types"); 
//                    resp.EnsureStatusIsSuccessful();
                    DocType doc = DocType.readAsXelement(resp.Content.ReadAsXElement());
                    if (doc != null) {
                        map.Add(doc.id, doc);
                    }
                }

            }
            return map;
        }


        public String GetLatestVersionIdFromServer(string bundleId) {
            MetaData meta = this.GetMetaFromBundle(bundleId);
            if (meta.keyValueData.ContainsKey("bundle.active")) {
                if (meta.keyValueData["bundle.active"] == "false") return null;
            }
            return meta.keyValueData.ContainsKey("bundle.has.new.version") ? GetLatestVersionIdFromServer(meta.keyValueData["bundle.has.new.version"]) : bundleId;
        }


        public Bundle GetLatestVersionFromServer(String bundleid){
            string latest = GetLatestVersionIdFromServer(bundleid); 
            return (latest == null ) ?   null :   getBundleInfo(latest); 
        }

        private static String _getRDFName(String s) { return "{http://www.w3.org/1999/02/22-rdf-syntax-ns#}" + s; }
        private static String _getSkoName(String s) { return "{http://www.w3.org/2004/02/skos/core#}" + s; }
        private static String _getLocName(String s) {
            return "{http://www.loc.gov/zing/srw/}" + s;
        }

        private void _buildSubTree(XElement rootElement, Tag parent, Scheme scheme) {
            IEnumerable<Tuple<string, string>> childs = _getChilds(rootElement, parent.WeeId);
            foreach (Tuple<String, String> tuplo in childs) {
                string path = parent.Path + "\\" + tuplo.Item2; // children path
                Tag children = new Tag(tuplo.Item2, path, tuplo.Item1);
                scheme.arvore.add(children, parent.Path, children.Path);
                scheme.arvoreByWeeboxIds.add(children, parent.WeeId, children.WeeId);
                _buildSubTree(rootElement, children, scheme);
            }
        }

        /**
         * el must have the type Concept
         */
        private Tuple<String, String> _getIdAndLabelFromTag(XElement el) {
            //let's get his id and 
            String id, label;
            label = el.Element(_getSkoName("prefLabel")).Value;
            id = el.Attribute(_getRDFName("about")).Value;
            return new Tuple<String, String>(id, label);
        }

        private IEnumerable<Tuple<string, string>> _getFirstLevelChilds(XElement root) {
            List<Tuple<String, String>> lista = new List<Tuple<String, String>>();

            foreach (XElement elem in root.Descendants(_getSkoName("Concept"))) {
                if (elem.Element(_getSkoName("broader")) == null) {
                    lista.Add(_getIdAndLabelFromTag(elem));
                }
            }
            return lista;
        }


        private IEnumerable<Tuple<string, string>> _getChilds(XElement root, string parent) {
            List<Tuple<String, String>> lista = new List<Tuple<String, String>>();
            foreach (XElement con in root.Descendants(_getSkoName("Concept"))) {
                foreach (XElement elem in con.Descendants(_getSkoName("broader"))) {
                    foreach (XAttribute x in elem.Attributes()) {
                        if (x.Value.Equals(parent)) {
                            if (x.Name.LocalName.Equals("resource")) {
                                lista.Add(_getIdAndLabelFromTag(con));
                            }
                        }
                    }
                }
            }
            return lista;
        }



        public Tuple<String, List<String>> RemoveFicheiros(String bundleid, List<Ficheiro> files) {
            List<String> file_names = files.ConvertAll((Ficheiro x) => x.md5);
            return this.RemoveFicheiros(bundleid, file_names);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleId"></param>
        /// <param name="files"></param>
        /// <returns>Bundle id , removed files</returns>
        public Tuple<String, List<String>> RemoveFicheiros(String bundleId, List<String> files) {
            string fullResponse = bundleId;
            List<String> removed = new List<string>(files);
            foreach (String file in files) {
                string postUrl =
                "http://photo.weebox.keep.pt/core/bundle/" + fullResponse + "?operation=updateFiles";
                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                postParameters.Add("removeFile", file);
                HttpWebResponse webResponse = MultipartFormDataPost(postUrl, postParameters);
                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                fullResponse = responseReader.ReadToEnd();
                removed.Remove(file);
            }
            return new Tuple<String, List<String>>(fullResponse, removed);
        }
 

        public String PutFicheiro(string weeId, Ficheiro  file){
            return PutFicheiros(weeId, new List<Ficheiro>() {file}); 
        }
 
 
        public Tuple<String,List<String>> RemoveFicheiro(String weeId, String fileId){
            return RemoveFicheiros(weeId, new List<String>(){fileId}); 
        }


        /// <summary>
        /// </summary>
        /// <param name="bundleid"></param>
        /// <param name="files"></param>
        public String PutFicheiros(String bundleid, List<Ficheiro> files) {
            Bundle bundle = getBundleInfo(bundleid);
            //Remover ficheiros com md5s iguais de files e corrigir nomes
            if (bundle == null) return null; 

            foreach (Ficheiro file in bundle.filesPath) {
                files.RemoveAll((Ficheiro x) => x.md5 == file.md5);
                IEnumerable<Ficheiro> files_repetidos = files.FindAll((Ficheiro x) => x.name == file.name);
                int i = 0;
                foreach (Ficheiro dup in files_repetidos) {
                    dup.name = dup.name + "_Duplicate_copy_" + 0;
                }
            }

            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("bundle.has.new.version", "undefined");
            foreach (Ficheiro file in files) {
                // Read file data
                FileStream fs = new FileStream(file.path, FileMode.Open, FileAccess.Read);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                postParameters.Add("encoded.filename=" + file.name, file.name);
                postParameters.Add(file.name, new FileParameter(data, file.name, "charset=UTF-8"));
            }

            string postUrl =
                     "http://photo.weebox.keep.pt/core/bundle/" + bundleid + "?operation=updateFiles";

            HttpWebResponse webResponse = MultipartFormDataPost(postUrl, postParameters);
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();

            return fullResponse;
        }

        private static readonly Encoding encoding = Encoding.UTF8;
        private long _boundary = 28947758029299;

        public HttpWebResponse MultipartFormDataPost(string postUrl, Dictionary<string, object> postParameters) {
            long value = this._boundary -= 1;
            string formDataBoundary = "-----------------------------" + value;
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters,formDataBoundary);

            return PostForm(postUrl, contentType, formData);
        }

        private HttpWebResponse PostForm(string postUrl, string contentType, byte[] formData) {

            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null) {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties
            request.Method = "POST";
            request.Credentials = _client.TransportSettings.Credentials;
            request.PreAuthenticate = true;
            request.Proxy = _client.TransportSettings.Proxy; 
            request.ContentType = contentType;
            request.ContentLength = formData.Length;  // We need to count how many bytes we're sending. 
            request.KeepAlive = true;
            request.Pipelined = true;
            request.AllowWriteStreamBuffering = true;
            request.SendChunked = true;
            if (formData.Length >= (1024 * 1024)) {
                request.Timeout = 1000 * formData.Length * 8 / 10000;
            }

            using (Stream requestStream = request.GetRequestStream()) {
                // Push it out there
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }
            return request.GetResponse() as HttpWebResponse;
        }


        private byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary) {
            Stream formDataStream = new System.IO.MemoryStream();
            foreach (var param in postParameters) {
                if (param.Value is FileParameter) {
                    FileParameter fileToUpload = (FileParameter)param.Value;
                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
                     boundary,
                     param.Key,
                     fileToUpload.FileName ?? param.Key,
                     fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, header.Length);

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                    // Thanks to feedback from commenters, add a CRLF to allow multiple files to be uploaded
                   // formDataStream.Write(encoding.GetBytes("\r\n"), 0, 2);
                }
                else {
                    if (param.Key == "removeFile") {
                        string postData =
                            //  string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
                            string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                                          boundary,
                                          param.Key,
                                          param.Value);
                        formDataStream.Write(encoding.GetBytes(postData), 0, postData.Length);
                    }
                    else {
                        string postData =
                              string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
                            //string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                                          boundary,
                                          param.Key,
                                          param.Value);
                        formDataStream.Write(encoding.GetBytes(postData), 0, postData.Length);
                    }
                }
            }

            // Add the end of the request
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, footer.Length);

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();
            return formData;
        }

        public class FileParameter {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public FileParameter(byte[] file) : this(file, null) { }
            public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
            public FileParameter(byte[] file, string filename, string contenttype) {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }


        private void _checkAndThrowsExceptions(HttpStatusCode statusCode, string context) {
            switch (statusCode) {
                case HttpStatusCode.OK:
                    return;
                case HttpStatusCode.Created:
                    return;
                case HttpStatusCode.Unauthorized:
                    throw new AccessDeniedException();
                case HttpStatusCode.InternalServerError:
                    throw new ServerErrorException();
                default:
                    throw new HttpError(statusCode, context);
            }
        }

        public bool GetFicheiro(String bundleId, String md5, String downloadPath){
            HttpResponseMessage resp = null;
            bool bo = true;
            while (bo){
                try{
                    resp =
                        _client.Get("core/file/" + bundleId + "/ " + md5 + "?encodeFileName=true");
                                _checkAndThrowsExceptions(resp.StatusCode, "download file"); 
//                    resp.EnsureStatusIsSuccessful(); 
                    bo = false;
                    
                }

                catch (HttpStageProcessingException e){
                    continue;
                }
            }

            Stream files = resp.Content.ReadAsStream();
            string file_path = downloadPath;
            FileStream f = File.OpenWrite(file_path);
            files.CopyTo(f);
            f.Flush();
            f.Close();
            List<Ficheiro> ficheiros = new List<Ficheiro>();
            ficheiros.Add(new Ficheiro(file_path, bundleId, true));
            return true;
            }
        }
    }


