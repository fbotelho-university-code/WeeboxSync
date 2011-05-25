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
    public partial class LoginWindow : Form
    {
        private WeeboxSync weebox;

        private string user;
        private string server;
        private string password;

        public LoginWindow(WeeboxSync weeboxInstance)
        {
            InitializeComponent();
            
            this.weebox = weeboxInstance;
            user = "";
            password = "";

            server = @"http://photo.weebox.keep.pt/";

            

        }

        private void iniciarButton_Click(object sender, EventArgs e) {
            if(weebox.connection_info == null)
                weebox.connection_info = new ConnectionInfo();
            
            weebox.connection_info.user.user = usernameTextBox.Text;
            weebox.connection_info.user.pass = passwordTextBox.Text;

            //TODO test connection and credentials
            
            //if(credentials ok)
            //{
            //    this.Visible = false;
            //    this.Enabled = false;
            //} else
            //{
            //    MessageBox.Show ("Erro ao estabelecer ligacao." +
            //                     "Verificar credenciais e servidor", "Erro de ligacao");
            //}
        }

        private void button1_Click(object sender, EventArgs e) {
            ConnectionInfo con = weebox.connection_info;
            if(con == null)
                con = new ConnectionInfo ();

            ConnectionInfoEditor cie = new ConnectionInfoEditor (con);
            cie.ShowDialog (this);

        }
    }
}

