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
    using System.Threading;

    public partial class DownloadWait : Form {
        private WeeboxSync weebox;
        public DownloadWait(ref WeeboxSync weeboxInst)
        {
            InitializeComponent();
            weebox = weeboxInst;
        }
        private void DownloadWait_Shown(object sender, EventArgs e)
        {
            BackgroundWorker backgroundWorker1 = new BackgroundWorker ();
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            backgroundWorker1.RunWorkerAsync();
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            weebox.setup();
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled) {
                this.DialogResult = DialogResult.Cancel;
            }
            else if (e.Error != null) {
                this.DialogResult = DialogResult.Cancel;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
            this.Visible = false;
            this.Close ();
        }
    }
}
