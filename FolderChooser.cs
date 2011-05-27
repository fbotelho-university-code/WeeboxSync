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
    public partial class FolderChooser : Form {
        
        private WeeboxSync weebox;

        public FolderChooser(ref WeeboxSync weeboxInstance)
        {
            InitializeComponent();
            weebox = weeboxInstance;
            rootFolderTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog ();
            fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fbd.ShowDialog ();
            
            rootFolderTextBox.Text = fbd.SelectedPath;
        }

        private void continueButton_Click(object sender, EventArgs e) {
            var res = weebox.setRootFolder (rootFolderTextBox.Text);
            int min;
            int.TryParse (minuteTextBox.Text, out min);




            //TODO - tocha - create method in WeeboxSync, use tryParse
            //weebox.setDefaultInterval (min);
            this.DialogResult = DialogResult.OK;
            this.Close ();
        }

        private void backButton_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Retry;
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




    }
}
