using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeeboxSync
{
    using System.Threading;

    class ProgramMain
    {
        private static volatile WeeboxSync Weebox;
        private static readonly object SyncLock = new object ();
        private static volatile bool Syncing;
        private static int DefaultInterval; //in minutes

        public static void CheckSync() {
            while(true) {
                lock(SyncLock) {

                    if(Syncing) {
                        Thread.Sleep (TimeSpan.FromMinutes (DefaultInterval));
                    }else {
                        Syncing = true;
                        
                    }
                }
            }


        }

        public static void Main() {





            //verificar se é a primeira vez que corremos o prog, se sim, setup
            //Weebox = new WeeboxSync();

            ////Create login window
            //LoginWindow login = new LoginWindow(Weebox);

            //Application.Run(login);

            ////Create the tray icon
            //Thread trayThread = new Thread (() => {
            //                           TrayApp ta = new TrayApp ();
            //                       });
        }
    }
}
