using System;
using System.Data.SqlClient;

namespace WeeboxSync {
    public class ConnectionInfo {
        private Utilizador utilizador;
        public Uri address { get; set; }
        public Uri proxy { get; set; }
        public bool useProxy { get; set; }  // guardar na bd também 

        public ConnectionInfo(){
            this.utilizador.user = ""; 
            this.utilizador.pass = "";
            this.proxy  = null;
            this.address  = null;
        }

        public ConnectionInfo( Utilizador u , string server, string proxy){
            this.user = u; 
            this.address = new Uri(server);
            this.proxy = new Uri(proxy);
            this.useProxy = true; 
        }

        public Utilizador user { get; set; }
        /**
         * base address SHALL NOT contain /core
         */ 

        public ConnectionInfo(Utilizador user, string baseAddress) {
            this.address = new Uri(baseAddress);
            this.user = user; 
        }
        public ConnectionInfo(Utilizador user , Uri baseAddress) {
            this.user = user; this.address = baseAddress; 
        }

    }
}
