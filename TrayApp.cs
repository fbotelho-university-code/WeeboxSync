using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeeboxSync
{
    class TrayApp : Form
    {
        private WeeboxSync weebox;

        private NotifyIcon  trayIcon;
        private ContextMenu trayMenu;

        public TrayApp(ref WeeboxSync instance)
        {
            weebox = instance;
            trayMenu = new ContextMenu();
            var menu = trayMenu.MenuItems;
            MenuItem m = new MenuItem("Force Sync", ForceSync);
            menu.Add(0, m);
            m = new MenuItem("Exit", OnExit);
            menu.Add (1, m);
            trayIcon = new NotifyIcon {
                                          Text = "Weebox-Sync",
                                          Icon =
                                              new Icon(@"Icons\weebox_tray_icon2.ico"),
                                          ContextMenu = trayMenu,
                                          Visible = true
                                      };
        }

        private void ForceSync(object sender, EventArgs eventArgs)
        {
            if (!weebox.SynchronizeAll())
                MessageBox.Show("Sincronização já está a decorrer.\nAguarde algum tempo e tente novamente.");
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            //maybe do some stuff before exiting?
            var res = MessageBox.Show (
                "Tem a certeza que deseja sair?",
                "Confirmação",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if(res == DialogResult.OK)
                Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}

