using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeeboxSync
{
    using System.Threading;
    using Microsoft.Win32;

    class ProgramMain
    {
        private static volatile WeeboxSync Weebox;
        private static readonly object SyncLock = new object ();
        private static volatile bool Syncing;
        private static int DefaultInterval; //in minutes
        private static bool setupOK;

        public static void CheckSync() {
           /* while(true) {
                lock(SyncLock) {

                    if(Syncing) {
                        Thread.Sleep (TimeSpan.FromMinutes (DefaultInterval));
                    }else {
                        Syncing = true;
                        
                    }
                }
            }*/
        }

        private static bool IsFirstRun() {
            var value = Microsoft.Win32.Registry.GetValue(
                                 /*key*/    @"HKEY_CURRENT_USER\Software\KeepSolutions\Weebox",
                               /*value*/    "rootFolder",
                /*default return value*/    null);
            return (value == null);
        }

        private static bool SaveRegistryKeys() {
            Microsoft.Win32.RegistryKey key;
            try {
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey (@"Software\KeepSolutions\Weebox");
                key.SetValue ("username", Weebox.connection_info.user.user);
                key.SetValue ("rootFolder", Weebox.getRootFolder ());
                //TODO - tocha - save time interval
                key.SetValue ("syncInterval", Weebox.DefaultSyncInterval, RegistryValueKind.DWord);
                
                key.Close ();
            } catch(Exception e) {
                return false;
            }
            return true;
        }

        public static bool DoSetup()
        {
            ConnectionInfo ci = null;
            Weebox = new WeeboxSync();
            int state = 1;
            bool cont = true;
            DialogResult result;

            /**
             * Show welcome screen
             */
            do {
               /**
                * if dialog results are DialogResult.Retry,
                * show the last form (back button was pressed)
                * if the result is Cancel, setup is to be canceled entirely
                * if the result is OK, follow to the next form, up until download.
                */
                
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
                        //setup successfull, go to download
                        SaveRegistryKeys ();
                        Weebox.setup ();
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

        [STAThread]
        public static void Main() {
            setupOK = true;
            if (IsFirstRun()) {
                setupOK = DoSetup();
            }
            if (!setupOK) {
                Application.Exit();
            }

            ////Create the tray icon
            Thread trayThread = new Thread(() =>
            {
                TrayApp ta = new TrayApp();
                Application.Run(ta);
            });
            trayThread.Start();
            trayThread.Join ();
            //iniciar sync timer

            //else run!!!


        }
    }
}
