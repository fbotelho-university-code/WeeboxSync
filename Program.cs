//[assembly: ContractNamespace("", ClrNamespace="WeeboxSync")]
using System.Collections.Generic;
using System.IO;

namespace WeeboxSync {
    using System;
    using System.Windows.Forms;

    public class Testes
    {

        public static void setup(){
            ConnectionInfo con = new ConnectionInfo(
                new Utilizador("g10_demo", "demo"),
                "http://photo.weebox.keep.pt/", "http://proxy.uminho.pt/");

            
            CoreAbstraction core = CoreAbstraction.getCore();

            WeeboxSync wee = new WeeboxSync();
            FicheiroSystemAbstraction abs = new FicheiroSystemAbstraction();
            DataBaseAbstraction ab = new DataBaseAbstraction();
            ab.deleteALl();
            if (Directory.Exists(@"c:\users\fabiim\my documents\my weebox")){
                abs.DeleteRecursiveFolder(@"c:\users\fabiim\my documents\my weebox");
            }
            wee.connection_info = con; 
            wee.setDefaultRootFolder();
            Watcher w= new Watcher(ref wee, wee.getRootFolder() + "\\Bundles");
            wee.SetWatcher(ref w); 
           w.Disable();
           while (true){
               try{
                   wee.setup();
                   while (true){
                       wee.SynchronizeAll();
                   }
               }
               catch (Exception e){
                   Console.Out.Write("Exception");
                   Console.Out.Write(e);
               }
           }
            try{
                wee.setup();


                while (true){
                    wee.SynchronizeAll();
                }
            }catch (Exception e){
                Console.Out.Write("Exception");
                Console.Out.Write(e);
                Console.Read(); 

            }
            /*            //wee.getBundleInfo("2BF883C16A5D8C2E827CD47A6EA19028"); 
            Ficheiro file = new Ficheiro(@"c:\asd.png", true);
            List<Ficheiro> files = new List<Ficheiro>(){ new Ficheiro(@"c:\asd.png", true), new Ficheiro(@"c:\drf.png", true) };
            wee.PutFicheiros("689421950C2542A0A7E09C76E40536C4", files); 
            */
            //wee.RemoveFicheiros(
//            wee.RemoveFicheiros("E37ADFC66E0ECB631998DFB13B34BB63",
//                                new List<string>()
//                                    {
                                    //    "1F8FEC6875461D0B5BA6EA7972486F8B",
//                                        "66BF8E7E5DCAF1D96FEE3EA78D8549CD"
  //                                  }); 

            Console.ReadLine(); 
//            wee.PutFicheiro("", new Ficheiro());

            //con.user
            //con.proxy.ToString(); 

//            WeeboxSync weebox = new WeeboxSync();
            //          weebox.connection_info = con;
            //        weebox.setup();
//            CoreAbstraction core = CoreAbstraction.getCore();
//            core.SetConnection(con);

//            core.PutFicheiro("", new Ficheiro());

        }

        private static void alter(Tag tag){
            tag.Name = tag.Path = tag.WeeId = "ola"; 

        }

        public static void Main(){ setup();}

        //public static void Main() {
            //ConnectionInfo conI2 = new ConnectionInfo("toukax", "123456", "cenas", "3128", "nao tem");
            //verificar se é a primeira vez que corremos o prog, se sim, setup
            //weebox = new WeeboxSync();

            ////Create login window
            //LoginWindow login = new LoginWindow(weebox);

            //Application.Run(login);
            ////now we should already have weebox
            ////
            //if (weebox == null) {
            //    //setup failed, exit gracefully!
            //    MessageBox.Show ("Returned null!!");
            //    return;
            //}
            //MessageBox.Show("Exited...");





    
            #region codigo antigo

            /*
            ConnectionInfo con = new ConnectionInfo(new Utilizador("admin", "4dm1n"), "http://photo.weebox.keep.pt");
            CoreAbstraction weebox = new CoreAbstraction();
            weebox.SetConnection(con);
            try {
                HttpClient _client = weebox._client;
                HttpResponseMessage resp = new HttpResponseMessage();
                //_client.Get("bundle/" + "8E239724985D402FD33D4C0C9BD452B4" + "?operation=retrieveBundleMetadata");
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
           
                dir = System.IO.Path.Combine(dir, "bundles");

                weebox.tmpPath = dir;
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }


                weebox.getBundle("64BF7CDDA4E07EB93CB2E57F04F976E4");
                System.Console.Read(); 
                //             FileStream f = File.OpenRead(dir + "\\1B98E870C9998CE010D609E09EDB603D.zip"); 
/*                weebox.getBundle("1B98E870C9998CE010D609E09EDB603D");
                weebox.getBundle("CFA63C7EFC32EACF18D67394E19A5C48");
                weebox.getBundle("6304095068FA2BEDE2DA79B13F520D11");
                weebox.getBundle("64BF7CDDA4E07EB93CB2E57F04F976E4"); 
                weebox.getBundle("44DA735507F09D101011C262318678C2");
*/
            /*                using (ZipFile zip = ZipFile.Read(f)){
                                zip.ExtractAll(dir);
                              }*/

            /*
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
                 * */
            /*            }
                        catch (Exception e) {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(e);
                            Console.ReadKey();
                        }

                        */

            #endregion
        //}


    }
}


        


 