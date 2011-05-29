using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace WeeboxSync {

    public class Bundle {
        public DocType type;
        public string weeId { get; set; }
        public string localId { get; set;  }
        public List<String> weeTags { get; set; }
        public MetaData meta { get ; set ; }
        public List<Ficheiro> filesPath { get; set;  }

        public Bundle() {
            this.weeId = "";
            this.localId = "";
            this.weeTags= new List<string>();
            this.meta = new MetaData();
            this.filesPath = new List<Ficheiro>();

        }

        public String getPath(string root){
            return root + "\\" + localId; 
        }

    }
}