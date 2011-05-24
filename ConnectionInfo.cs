using System;
using System.Data.SqlClient;

namespace WeeboxSync {
    public class ConnectionInfo {

        public string username;
        public string password;
        public string server_address;
        public string server_port;
        public string proxy_address;

        public ConnectionInfo() {
            this.username = "";
            this.password = "";
            this.server_address = "";
            this.server_port = "";
            this.proxy_address = "";
        }

        public ConnectionInfo(string u, string p, string sa, string sp, string pa)
        {
            this.username = u;
            this.password = p;
            this.server_address = sa;
            this.server_port = sp;
            this.proxy_address = pa;
        }

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

        public void writeConnection(ConnectionInfo conI) {
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                SqlCommand query = new SqlCommand("INSERT INTO utilizador (username, password, server_address, server_port, proxy) VALUES ('" + conI.username + "', '" + conI.password + "', '" + conI.server_address + "', '" + conI.server_port + "', '" + conI.proxy_address + "')", con);
                query.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            //SqlCommand query = new SqlCommand("INSERT INTO utilizador (username, password, server_address, server_port, proxy) VALUES ('" + conI.username + "', '" + conI.password + "', '" + conI.server_address + "', '" + conI.server_port + "', '" + conI.proxy_address + "')", con);
            //query.ExecuteNonQuery();
            try
            {
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



    }
}
