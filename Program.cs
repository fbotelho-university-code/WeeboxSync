using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.Http; 
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Syndication;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Net;
using System.Xml;
using System.IO;
using Ionic; 
using System.Text.RegularExpressions; 


//[assembly: ContractNamespace("", ClrNamespace="WeeboxSync")]
namespace WeeboxSync {

    public class Testes {
        public static void Main() {
            System.Console.WriteLine("cenas delvjbnln");
            System.Console.ReadLine();
         /*   ConnectionInfo con = new ConnectionInfo(new Utilizador("admin", "4dm1n"), "http://photo.weebox.keep.pt");
            CoreAbstraction weebox = new CoreAbstraction();
            weebox.SetConnection(con);
            try {
                HttpClient _client = weebox._client;
                HttpResponseMessage resp = new HttpResponseMessage() ;//_client.Get("bundle/" + "8E239724985D402FD33D4C0C9BD452B4" + "?operation=retrieveBundleMetadata");


//                HttpResponseMessage resp = _client.Get
                String tryString = "Q2W1C42bT6 :: peNmOsZpdJ :: TrHukNxZub :: VpsMUmd783 ;; Q2W1C42bT6 :: peNmOsZpdJ :: TrHukNxZub";
                


                String[] strs =Regex.Split(tryString, ";;"); 
                foreach (String st in strs){
                    String[] tagss = Regex.Split(st, "::"); 
                    Console.WriteLine(tagss.Last<String>().Trim()); 
                }

                resp.EnsureStatusIsSuccessful();

                Stream s = resp.Content.ReadAsStream();
                XDocument meta = XDocument.Load(s);
                XElement rootElement = meta.Root;

                foreach (XElement e in rootElement.Descendants()) {
                    if (!e.Value.Equals("")) {
                        Console.WriteLine(e.FirstAttribute.Value + "-----> " + e.Value);
                    }
                }
                // create bundle 
                // perceber como são associados a tags, criar ficheiros, metadata
                Console.ReadKey();
            }
            catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(e);
                Console.ReadKey();
            }
          */
        }
    }
}

        


 