using System;
using System.Windows.Forms;

namespace WeeboxSync
{
    using System.Threading;
    using Microsoft.Win32;

    class ProgramMain
    {
        private static WeeboxSync Weebox;
        private static bool _setupOk;
        private static int _defaultInterval; //in minutes
        private static string _username;
        private static string _rootFolder;

        [STAThread]
        public static void Main() {
            #region main

            _setupOk = true;
            
            if (IsFirstRun())
            {
                _setupOk = DoSetup();
            }
            else {
                //iniciar nova instância
                Weebox = new WeeboxSync ();
                DataBaseAbstraction dba = new DataBaseAbstraction ();
                ConnectionInfo ci = null;
                try{
                    GetRegistryKeys();
                    ci = dba.GetConnectionInfo (_username);
                } catch (Exception e) {
                    MessageBox.Show("Erro ao recuperar credenciais de utilizador.", "Erro");
                    return;
                }
                Weebox.connection_info = ci;
                Weebox.saveRootFolder (_rootFolder);
                Weebox.DefaultSyncInterval = _defaultInterval;

            }//end else

            if (!_setupOk) {
                return;
            }

            //create sync timer thread
            Thread timer = new Thread (CheckSync);
            //if the parent thread terminates, all the background threads terminate too
            timer.IsBackground = true;
            timer.Start();

            //Create the tray icon
            TrayApp ta = new TrayApp(ref Weebox);
            Application.Run(ta);

            //Create watcher
            Thread t = new Thread (StartWatcher) {IsBackground = true};
            t.Start();
      
            #endregion

            #region tests

            /*
            string wid = "tag_id_1";

            Tag t0 = new Tag("p", "/p", "tag_id_0");
            Tag t1 = new Tag("t1", "/p/t1", "tag_id_1");
            Tag t2 = new Tag("t2", "/p/t1/t2", "tag_id_2");
            Tag t3 = new Tag("t3", "/p/t1/t2/t3", "tag_id_3");
            Tag t4 = new Tag("t4", "/p/t1/t2/t4", "tag_id_4");
            Tag t5 = new Tag("t5", "/p/t1/t2/t5", "tag_id_5");
            Tag t6 = new Tag("t6", "/p/t1/t6", "tag_id_6");
            Tag t7 = new Tag("t7", "/p/t1/t6/t7", "tag_id_7");
            Tag t8 = new Tag("t8", "/p/t1/t6/t8", "tag_id_8");
            Tag t9 = new Tag("t9", "/p/t9", "tag_id_9");
            Tag t10 = new Tag("t10", "/p/t9/t10", "tag_id_10");
            Tag t11 = new Tag("t11", "/p/t9/t11", "tag_id_11");
            Tag t12 = new Tag("t12", "/p/t9/t12", "tag_id_12");
            Tag t13 = new Tag("t13", "/p/t13", "tag_id_13");
            Tag t14 = new Tag("t14", "/p/t13/t14", "tag_id_14");
            Tag t15 = new Tag("t14", "/p/t13/t14/t15", "tag_id_15");
            Tag t16 = new Tag("t14", "/p/t13/t14/t15/t16", "tag_id_16");
            Tag t17 = new Tag("t14", "/p/t13/t14/t15/t16/t17", "tag_id_17");
            Tag t18 = new Tag("t14", "/p/t13/t14/t15/t16/t17/t18", "tag_id_18");
            
            Scheme sc = new Scheme ("scheme_0", t0);
            sc.arvore.add (t1, t0.Path, t1.Path);
            sc.arvore.add(t2, t1.Path, t2.Path);
            sc.arvore.add(t3, t2.Path, t3.Path);
            sc.arvore.add(t4, t3.Path, t4.Path);
            sc.arvore.add(t5, t4.Path, t5.Path);
            sc.arvore.add(t6, t5.Path, t6.Path);
            sc.arvore.add(t7, t6.Path, t7.Path);
            sc.arvore.add(t8, t7.Path, t8.Path);
            sc.arvore.add(t9, t8.Path, t9.Path);
            sc.arvore.add(t10, t9.Path, t10.Path);
            sc.arvore.add(t11, t10.Path, t11.Path);
            sc.arvore.add(t12, t11.Path, t12.Path);
            sc.arvore.add(t13, t12.Path, t13.Path);
            sc.arvore.add(t14, t13.Path, t14.Path);
            sc.arvore.add(t15, t14.Path, t15.Path);
            sc.arvore.add(t16, t15.Path, t16.Path);
            sc.arvore.add(t17, t16.Path, t17.Path);
            sc.arvore.add(t18, t17.Path, t18.Path);

            Tag t110 = new Tag("t", "/t", "tag_id_110");
            Tag t111 = new Tag("t111", "/t/111", "tag_id_111");
            Tag t112 = new Tag("t112", "/t/112", "tag_id_112");
            Tag t113 = new Tag("t113", "/t/111/113", "tag_id_113");
            Tag t114 = new Tag("t114", "/t/111/114", "tag_id_114");
            Tag t115 = new Tag("t115", "/t/112/115", "tag_id_115");

            Scheme sc1 = new Scheme ("scheme_2", t110);
            sc1.arvore.add(t111, t110.Path, t111.Path);
            sc1.arvore.add(t112, t111.Path, t112.Path);
            sc1.arvore.add(t113, t112.Path, t113.Path);
            sc1.arvore.add(t114, t113.Path, t114.Path);
            sc1.arvore.add(t115, t114.Path, t115.Path);
            
            List<Scheme> lista = new List<Scheme> (2);
            lista.Add (sc);
            lista.Add (sc1);

            dba.SaveClassificationScheme (lista);

            lista = null;
            lista = dba.GetClassificationScheme ();

            Console.WriteLine("breakpoint");

              
              
            string bid = "id1";

            List<Ficheiro> l = new List<Ficheiro> ();
            for(int i = 0; i<40; i++) {
                Ficheiro f = new Ficheiro("path_"+ i, bid, "md5_"+i);
                l.Add (f);
            }

            Bundle bd = new Bundle {filesPath = l, localId = bid, meta = null, weeId = bid, weeTags = null};

            dba.SaveBundle (bd);

            dba.DeleteBundle (bd.localId);

            Console.WriteLine("breakpoint");
            */
            /*
            Bundle received = dba.getBundle (bid);

            IList<Ficheiro> listFicheiros = dba.GetFicheirosIDS (bid);
            */

            #endregion
        }

        private static void StartWatcher()
        {
            Watcher w = new Watcher(ref Weebox, Weebox.getRootFolder () + @"\Bundles");
            Weebox.SetWatcher (ref w);
        }

        private static void GetRegistryKeys() {
            _rootFolder = (string) Microsoft.Win32.Registry.GetValue(
                /*key*/    @"HKEY_CURRENT_USER\Software\KeepSolutions\Weebox",
                /*value*/    "rootFolder",
                /*default return value*/    "");
            _username = (string) Microsoft.Win32.Registry.GetValue(
                /*key*/    @"HKEY_CURRENT_USER\Software\KeepSolutions\Weebox",
                /*value*/    "username",
                /*default return value*/    "");
            _defaultInterval = (int) Microsoft.Win32.Registry.GetValue(
                /*key*/    @"HKEY_CURRENT_USER\Software\KeepSolutions\Weebox",
                /*value*/    "syncInterval",
                /*default return value*/    0);
        }

        private static void CheckSync() {
            Thread.Sleep (TimeSpan.FromMinutes (_defaultInterval));
            while(true) {
                Weebox.SynchronizeAll();
                Thread.Sleep(TimeSpan.FromMinutes(_defaultInterval));
            }
        }

        private static bool IsFirstRun() {
            var value = Microsoft.Win32.Registry.GetValue(
                    /*key*/    @"HKEY_CURRENT_USER\Software\KeepSolutions\Weebox",
                    /*value*/    "rootFolder",
                    /*default return value*/    null);
            return (value == null);
        }

        private static bool SaveRegistryKeys() {
            Microsoft.Win32.RegistryKey key = null;
            try {
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey (@"Software\KeepSolutions\Weebox");
                if(key == null)
                    return false;
                key.SetValue ("username", Weebox.connection_info.user.user);
                key.SetValue ("rootFolder", Weebox.getRootFolder ());
                key.SetValue ("syncInterval", Weebox.DefaultSyncInterval, RegistryValueKind.DWord);
            } catch (Exception e)
            {
                return false;
            } finally {
                if (key != null)
                    key.Close();
            }
            return true;
        }

        private static void DeleteRegistryKeys() {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\KeepSolutions\Weebox", true);
            if (key != null) {
                key.DeleteValue ("username");
                key.DeleteValue("rootFolder");
                key.DeleteValue("syncInterval");
                key.Close ();
            }
        }
        private static bool DoSetup()
        {
            ConnectionInfo ci = null;
            Weebox = new WeeboxSync();
            int state = 1;
            bool cont = true;

            //TODO - tocha - show welcome screen
            do {
                /**
                * if dialog results are DialogResult.Retry,
                * show the last form (back button was pressed)
                * if the result is Cancel, setup is to be canceled entirely
                * if the result is OK, follow to the next form, up until download.
                */

                DialogResult result;
                switch (state) {
                    case 1:
                        ci = new ConnectionInfo();
                        ConnectionInfoEditor cie = new ConnectionInfoEditor (ref ci);
                        result = cie.ShowDialog ();
                        if (result == DialogResult.Cancel)
                            return false;
                        if(result == DialogResult.Retry)
                            state = 0; // exit
                        else
                            state++;
                        break;
                    case 2:
                        if(ci == null) {
                            state = 0;
                            break;
                        }
                        Weebox.connection_info = ci;
                        LoginWindow lw = new LoginWindow ( ref Weebox);
                        result = lw.ShowDialog ();
                        if (result == DialogResult.Cancel)
                            return false;
                        if(result == DialogResult.Retry)
                            state--; // back to previous dialog
                        else
                            state++; // next dialog
                        break;
                    case 3:
                        FolderChooser fc = new FolderChooser (ref Weebox);
                        result = fc.ShowDialog ();
                        if (result == DialogResult.Cancel)
                            return false;
                        if (result == DialogResult.Retry)
                            state--; // back to previous dialog
                        else
                            state++; // next dialog
                        break;
                    case 4:
                        //setup successful, go to download
                        SaveRegistryKeys ();
                        DataBaseAbstraction dba = new DataBaseAbstraction ();
                        dba.SaveConnectionInfo (Weebox.connection_info);
                        try
                        {
                            Weebox.setup();
                        }
                        catch (Exception e) {
                            //eliminar chaves de registo

                            //eliminar pasta root
                            FicheiroSystemAbstraction fsa = new FicheiroSystemAbstraction ();
                            fsa.DeleteRecursiveFolder (Weebox.getRootFolder ());
                        }
                        cont = false;
                        break;
                    case 5:
                        // all tasks done, exit
                        
                        return true;
                    case 0:
                        //Cancel setup and exit application
                        Application.Exit ();
                        return false;
                    default:
                        MessageBox.Show (
                            "Houve um erro no processo de instalação.\n" + 
                            "Por favor volte a iniciar a aplicação para concluir o processo,",
                            "Erro",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        Application.Exit();
                        break;
                }
            } while (cont);
            MessageBox.Show ("Setup done!, Now we rock!");
            return true;
        }
    }
}
