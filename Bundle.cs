using System;
using System.Collections.Generic;
namespace WeeboxSync {

    public class Bundle {
        public string weeId { get; set; }
        public string localId { get; set;  }
        public List<String> weeTags { get; set; }
        public MetaData meta { get ; set ; }
        public IEnumerable<Ficheiro> filesPath { get; set;  }

    }

}