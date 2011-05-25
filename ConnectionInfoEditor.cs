using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeeboxSync
{
    public partial class ConnectionInfoEditor : Form {
        private ConnectionInfo connection;
        private string username;
        private string password;
        private string server = @"http://photo.weebox.keep.pt/";
        private bool useProxy;

        public ConnectionInfoEditor(ConnectionInfo con)
        {
            InitializeComponent();

            this.connection = con;

            server = @"http://photo.weebox.keep.pt/";
        }

        private void proxyCheckBox_CheckedChanged(object sender, EventArgs e) {
            var state = proxyCheckBox.Checked;
            useProxy = state;
            groupBox2.Enabled = state;

            

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            var state = proxyPassCheckBox.Checked;
            proxyUNameTextBox.Enabled = state;
            proxyPassTextBox.Enabled = state;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (useProxy) {
                connection.useProxy = true;
                Uri proxy = connection.proxy;
                //TODO - set proxy

                if (proxyPassCheckBox.Checked) {
                    //set proxy auth 
                }
            } else {
                connection.useProxy = false;
            }
            


        }
    }
}

