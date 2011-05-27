using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeeboxSync
{
    class TrayApp : Form
    {
        private WeeboxSync weebox;

        private NotifyIcon  trayIcon;
        private ContextMenu trayMenu;

        public TrayApp()
        {
            //weebox = instance;

            trayMenu = new ContextMenu();
            var menu = trayMenu.MenuItems;
            //menu.


            menu.Add("Force Sync", ForceSync);
            menu.Add("Exit", OnExit);
            

            trayIcon = new NotifyIcon();
            trayIcon.Text = "WeeboxSync\nStarting....";
            //trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);


            
            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

        }

        private void ForceSync(object sender, EventArgs eventArgs)
        {
            //TODO - force sync
            //Console.Error.WriteLine("Not yet implemented");
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

