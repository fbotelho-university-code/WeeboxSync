using System;
using System.Collections.Generic;
namespace WeeboxSync {

    public class Bundle {
        public string LocalId { get; set; }
        public string WeeId { get; set; }
        public List<String> WeeTags { get; set; }
        public MetaData Meta { get ; set ; }  

    }

}