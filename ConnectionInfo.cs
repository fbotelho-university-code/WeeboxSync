using System;
using System.Data.SqlClient;

namespace WeeboxSync {
    public class ConnectionInfo {
        public Utilizador user;
        public Uri address { get; set; }
        public Uri proxy { get; set; }
        public bool useProxy { get; set; }  // guardar na bd também 

        public ConnectionInfo(){
            user = new Utilizador ("", "");
            this.proxy  = null;
            this.address  = null;
        }
        public ConnectionInfo( Utilizador u , string server, string proxy){
            this.user = u;
            this.address = new Uri (server);
            this.proxy = new Uri (proxy);
            this.useProxy = true; 

        }

        public ConnectionInfo(Utilizador utilizador, string address) {
            this.user = utilizador;
            this.address = new Uri(address);
        }


    }
}
