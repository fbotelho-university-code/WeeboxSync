using System;
using System.Data.SqlClient;

namespace WeeboxSync
{
    public class Registo
    {
        public string op_id;
        public DateTime etiqueta;
        public string op_type;
        public string old_bundle_id;
        public string new_bundle_id;
        public string old_file_id;
        public string new_file_id;
        public string tag;

        public Registo(string oi, DateTime e, string ot, string obi, string nbi, string ofi, string nfi, string t) { 
            this.op_id = oi;
            this.etiqueta = e;
            this.op_type = ot;
            this.old_bundle_id = obi;
            this.new_bundle_id = nbi;
            this.old_file_id = ofi;
            this.new_file_id = nfi;
            this.tag = t;
        }

        public Registo()
        {
            this.op_id = "";
            this.etiqueta = DateTime.Now;
            this.op_type = "";
            this.old_bundle_id = "";
            this.new_bundle_id = "";
            this.old_file_id = "";
            this.new_file_id = "";
            this.tag = "";
        }
    }
}