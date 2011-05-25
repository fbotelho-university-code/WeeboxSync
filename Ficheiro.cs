using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WeeboxSync {
    public class Ficheiro {
              /// <summary>
                    //TODO - how to rep filestream?


    /// method for getting a files MD5 hash, say for
    /// a checksum operation
    /// </summary>
    /// <param name="file">the file we want the has from</param>
    /// throws something
    /// <returns></returns>
    public static  string getFilesMD5Hash(string file)
    {
        //open the file
        using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)){
            return Ficheiro.getFilesMD5Hash(stream); 
        }
    } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
    public static  string getFilesMD5Hash(Stream stream){
        //MD5 hash provider for computing the hash of the file
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider()){
            //open the file
            //calculate the files hash
            md5.ComputeHash(stream);
            //byte array of files hash
            byte[] hash = md5.Hash;


            //string builder to hold the results
            StringBuilder sb = new StringBuilder();

            //loop through each byte in the byte array
            foreach (byte b in hash){
                //format each byte into the proper value and append
                //current value to return value
                sb.Append(string.Format("{0:X2}", b));
            }

            return sb.ToString();
        }

    }

        public string bundleId { get; set; }
        public string md5 { get; set; }
        public string path { get; set; }


        /// <summary>
        /// Sets bundleId to null, initializes md5 if readMd5 says so reading from file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="readMd5" />
        public Ficheiro(string path, bool readMd5){
            if (readMd5){
                this.md5 = Ficheiro.getFilesMD5Hash(path);
            }
            this.path = path;
            this.bundleId = null;
        }

        public Ficheiro(string path, string bundleId, bool readMd5) {
            if (readMd5){
                this.md5 = Ficheiro.getFilesMD5Hash(path);
            }
            this.bundleId  = bundleId;
            this.path = path; 
        }

        public Ficheiro(string path , string bundleId , string md5){
            this.path = path;
            this.bundleId = bundleId;
            this.md5 = md5; 
        }

        public Ficheiro() {
            this.path = "";
            this.bundleId = "";
            this.md5 = "";
        }
    }
}