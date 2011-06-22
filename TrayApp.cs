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
            MenuItem m = new MenuItem("Forçar sincronização", ForceSync);
            menu.Add(0, m);
            m = new MenuItem("Alterar credenciais", SetUserCredentials);
            menu.Add (1, m);
            m = new MenuItem("Alterar dados de ligação", SetConnectionInfo);
            menu.Add(2, m);
            m = new MenuItem("Sair", OnExit);
            menu.Add (3, m);
            trayIcon = new NotifyIcon {
                                          Text = "Weebox-Sync",
                                          Icon =
                                              new Icon(@"Icons\weebox_tray_icon2.ico"),
                                          ContextMenu = trayMenu,
                                          Visible = true
            };
        }
        private void SetUserCredentials(object sender, EventArgs eventArgs)
        {
            WeeboxSync ws = new WeeboxSync ();
            LoginWindow lw = new LoginWindow(ref ws);
            var dialogRes = lw.ShowDialog();
            if (dialogRes == DialogResult.OK) { //nao foi cancelado
                var res = weebox.setCredentials (ws.connection_info.user.user, ws.connection_info.user.pass);
                if (!res) {
                    MessageBox.Show ("Sincronização em curso, por favor aguarde alguns minutos e tente novamente.",
                                     "Erro",
                                     MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void SetConnectionInfo(object sender, EventArgs eventArgs)
        {
            ConnectionInfo ci = new ConnectionInfo();
            ConnectionInfoEditor cie = new ConnectionInfoEditor(ref ci);
            var dialogRes = cie.ShowDialog();
            if (dialogRes == DialogResult.OK) { //not canceled
                var res = weebox.setConnectionInfo (ci.address, ci.proxy, ci.useProxy);
                if (!res)
                {
                    MessageBox.Show("Sincronização em curso, por favor aguarde alguns minutos e tente novamente.",
                                     "Erro",
                                     MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrayApp));
            this.SuspendLayout();
            // 
            // TrayApp
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TrayApp";
            this.ResumeLayout(false);

        }
    }
}

