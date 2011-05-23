using System;
namespace WeeboxSync {
    public class ConnectionInfo {
        public Utilizador user { get; set; }
        /**
         * base address SHALL NOT contain /core
         */ 
        public Uri address { get; set; }
        public Uri proxy { get; set; }
        public bool useProxy { get; set; }  // guardar na bd também 

        public ConnectionInfo(Utilizador user, string baseAddress) {
            this.address = new Uri(baseAddress);
            this.user = user; 
        }
        public ConnectionInfo(Utilizador user , Uri baseAddress) {
            this.user = user; this.address = baseAddress; 
        }
    }
}
