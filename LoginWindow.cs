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
        public LoginWindow(ref WeeboxSync weeboxInstance)
        {
            InitializeComponent();
            this.weebox = weeboxInstance;
        }

        private void iniciarButton_Click(object sender, EventArgs e)
        {
           if (weebox.connection_info == null)
               weebox.connection_info = new ConnectionInfo(new Utilizador("", ""), @"http://photo.weebox.keep.pt/");
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
            this.DialogResult = DialogResult.OK;
            this.Close ();
        }

        protected override void Dispose(bool disposing) {
            this.DialogResult = DialogResult.Abort;
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

