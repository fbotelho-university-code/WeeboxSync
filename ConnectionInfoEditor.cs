using System;
using System.Windows.Forms;

namespace WeeboxSync
{
    public partial class ConnectionInfoEditor : Form {
        private ConnectionInfo connection;
        private string server = @"photo.weebox.keep.pt/";

        public ConnectionInfoEditor(ref ConnectionInfo con)
        {
            InitializeComponent();
            connection = con;
            if (con != null && con.address != null) {
                // get values from con and fill form
                serverTextBox.Text = con.address.Host;
                serverPort.Text = con.address.Port.ToString ();
                if (con.useProxy) {
                    proxyCheckBox.Checked = true;
                    proxyServerTextBox.Text = con.proxy.Host;
                    proxyPortTextBox.Text = con.proxy.Port.ToString ();
                }
            } else {
                this.connection = con = new ConnectionInfo ();
                //fill with default values
                server = @"photo.weebox.keep.pt/";
                serverTextBox.Text = server;
                serverPort.Text = (80).ToString ();
                proxyCheckBox.Checked = false;
            }
        }

        private void proxyCheckBox_CheckedChanged(object sender, EventArgs e) {
            groupBox2.Enabled = proxyCheckBox.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (proxyCheckBox.Checked) {
                connection.useProxy = true;
                Uri proxy = new Uri (String.Format ("{0}:{1}",proxyServerTextBox.Text, proxyPortTextBox.Text));
            } else {
                connection.useProxy = false;
            }
            
            connection.address = serverPort.Text != String.Empty ?
                new Uri(String.Format("http://{0}:{1}", serverTextBox.Text, serverPort.Text)) :
                new Uri ("http://"+serverTextBox.Text);

            this.DialogResult = DialogResult.OK;
            this.Close ();
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void button1_Click_1(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Retry;
            this.Close ();
        }
    }
}