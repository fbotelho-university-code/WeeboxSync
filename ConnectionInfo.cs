using System;
using System.Data.SqlClient;

namespace WeeboxSync {
    public class ConnectionInfo {
        public Utilizador user; 
        public Uri address { get; set; }
        public Uri proxy { get; set; }
        public bool useProxy { get; set; }  // guardar na bd também 

        public ConnectionInfo(){
            this.user.user = ""; 
            this.user.pass = "";
            this.proxy  = null;
            this.address  = null;
        }


        public ConnectionInfo( Utilizador u , string server, string proxy){
            this.user = u; 
            this.address = new Uri(server);
            this.proxy = new Uri(proxy);
            this.useProxy = true; 


            public ConnectionInfo(string username, string password, string server_address, string server_port, string proxy, string boleano, int serial_generator, string folder){
                this.utilizador.user = username;
                this.utilizador.pass = password;
                this.address = server_address;
            }

    }

    }
