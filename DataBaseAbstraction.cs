using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Collections;

namespace WeeboxSync {
    public class DataBaseAbstraction {

        public Bundle getBundle(string id){
            throw new System.Exception("Not implemented");
        }

        public void DeleteBundle(String bundleId) {
            throw new System.Exception("Not implemented");
        }
        public void GetClassificationScheme() {
            throw new System.Exception("Not implemented");
        }
        public void SaveClassificationScheme(Scheme scheme) {
            throw new System.Exception("Not implemented");
        }
        public void UpdateClassificationScheme(Scheme scheme) {
            throw new System.Exception("Not implemented");
        }
        public List<String> GetAllBundles() {//todas as tabelas
            throw new System.Exception("Not implemented");
        }
        public void RmBundle(String bundleId) {
            throw new System.Exception("Not implemented");
        }











        public IList<Ficheiro> GetFicheirosIDS(String bundleId) {//todos os ficheiros k tem o bundleID -------->>> e o id-> outro primary key. nao interessa????
            Ficheiro file = new Ficheiro();
            IList<Ficheiro> lista;
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                SqlCommand query = new SqlCommand("select * from ficheiro where bundleID = '"+bundleId+"'", con);
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    file = new Ficheiro(reader.GetString(0), reader.GetString(1), reader.GetString(2));
                    lista.Add(file);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return lista;
        }

        public void SaveFicheiroInfo(Ficheiro ficheiro)
        {
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                try
                {
                    SqlCommand query = new SqlCommand("INSERT INTO ficheiro (id, bundleID, nome_ficheiro) Values ('" + ficheiro.md5 + "', '" + ficheiro.bundleId + "', '" + ficheiro.path + "')", con);
                    query.ExecuteNonQuery();
                }
                catch (Exception naoexistebundle)
                {
                    Console.WriteLine(naoexistebundle.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        
        public void RmFicheiroInfo(String Ficheiro, String bundleID) { //elimino tambem do bundle????????????
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                SqlCommand query = new SqlCommand("delete from ficheiro where id = '"+Ficheiro+"' and bundleID = '"+bundleID+"'", con);
                query.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SaveBundle(Bundle bundle)
        {
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                SqlCommand query = new SqlCommand("INSERT INTO bundle (id, version_ID) VALUES ('" + bundle.localId + "', '" + bundle.weeId + "')", con);
                query.ExecuteNonQuery();
                foreach (Ficheiro f in bundle.filesPath)
                {
                    query = new SqlCommand("INSERT INTO last_updated_bundles (bundle_version_ID, file_ID, filename) VALUES ('" + bundle.localId + "', '" + bundle.weeId + "', '" + f.path + "')", con);
                    query.ExecuteNonQuery();
                }
                foreach (Ficheiro f2 in bundle.filesPath)
                {
                    query = new SqlCommand("INSERT INTO ficheiro (id, BundleID, filename) VALUES ('" + f2.md5 + "', '" + bundle.localId + "', '" + f2.path + "')", con);
                    query.ExecuteNonQuery();
                }
                foreach (KeyValuePair<String, String> kvp in bundle.meta.keyValueData)
                {
                    String v1 = kvp.Key;
                    String v2 = kvp.Value;
                    query = new SqlCommand("INSERT INTO metadata (BundleID, field_name, field_data) VALUES ('" + bundle.localId + "', '" + v1 + "', '" + v2 + "')", con);
                    query.ExecuteNonQuery();
                }
                foreach (String tag in bundle.weeTags)
                {
                    query = new SqlCommand("INSERT INTO tags (BundleID, tag) VALUES ('" + bundle.localId + "', '" + tag + "')", con);
                    query.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SaveConnectionInfo(ConnectionInfo conI) {
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                SqlCommand query = new SqlCommand("INSERT INTO utilizador (username, password, server_address, server_port, proxy, serial_generator, folder) VALUES ('" + conI.username + "', '" + conI.password + "', '" + conI.server_address + "', '" + conI.server_port + "', '" + conI.proxy_address + "', "+conI.serial_genearator+", '"+conI.folder+"')", con);
                query.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public ConnectionInfo ReadConnectionInfo(String username) {
            ConnectionInfo conI = new ConnectionInfo();
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                SqlCommand query = new SqlCommand("SELECT * FROM utilizador WHERE username = '" + username + "'", con);
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    conI = new ConnectionInfo(username, reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt64(5), reader.GetString(6));
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return conI;
        }
        
        public void writeRegisto(Registo reg){
                string ConnectionString = "Data Source=(local);Integrated Security=True";
                SqlConnection con = new SqlConnection(ConnectionString);
                try
                {
                    con.Open();
                    SqlCommand query = new SqlCommand("INSERT INTO historico (op_id, op_type, old_bundle_id, new_bundle_id, old_file_id, new_file_id, tag, etiqueta) Values ('" + reg.op_id + "', '" + reg.op_type + "', '" + reg.old_bundle_id + "', '" + reg.new_bundle_id + "', '" + reg.old_file_id + "', '" + reg.new_file_id + "', '" + reg.tag + "', getdate())", con);
                    query.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                try
                {
                    con.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
        }

        public Registo readRegisto(string op_id)
        {
            Registo reg = new Registo();
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            SqlCommand query = new SqlCommand("SELECT * FROM historico WHERE op_id = '" + op_id + "'", con);
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                reg = new Registo(op_id, (DateTime) reader.GetDateTime(7), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6));
            }
            reader.Close();
            try
            {
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return reg;
    }
}
}