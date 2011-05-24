using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace WeeboxSync {
    public class DataBaseAbstraction {

        public void SaveBundle(Bundle bundle) {
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                SqlCommand query = new SqlCommand("INSERT INTO bundle (id, version_ID) VALUES ('" +bundle.localId+ "', '" +bundle.weeId+ "')", con);
                query.ExecuteNonQuery();
                foreach (Ficheiro f in bundle.filesPath) {
                    query = new SqlCommand("INSERT INTO last_updated_bundles (bundle_version_ID, file_ID, filename) VALUES ('"+bundle.localId+"', '"+bundle.weeId+"', '"+f.path+"')", con);
                    query.ExecuteNonQuery();
                }
                foreach (Ficheiro f2 in bundle.filesPath) {
                    query = new SqlCommand("INSERT INTO ficheiro (id, BundleID, filename) VALUES ('"+f2.md5+"', '"+bundle.localId+"', '"+f2.path+"')", con);
                    query.ExecuteNonQuery();
                }
                foreach (KeyValuePair<String, String> kvp in bundle.meta.keyValueData)
                {
                    String v1 = kvp.Key;
                    String v2 = kvp.Value;
                    query = new SqlCommand("INSERT INTO metadata (BundleID, field_name, field_data) VALUES ('"+bundle.localId+"', '"+v1+"', '"+v2+"')", con);
                    query.ExecuteNonQuery();
                }
                foreach(String tag in bundle.weeTags){
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

        /*public getBundle(string id){
        
        }*/

        public void DeleteBundle(String bundleId) {
            throw new System.Exception("Not implemented");
        }

        public void SaveFicheiroInfo(Ficheiro Ficheiro, String bundleId) {
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
        public void SaveConnectionInfo(ConnectionInfo connectionInfo) {
            throw new System.Exception("Not implemented");
        }

        public void SaveRootFolder(String path) {
            throw new System.Exception("Not implemented");
        }
        public List<String> GetAllBundles() {
            throw new System.Exception("Not implemented");
        }
        public void RmBundle(String bundleId) {
            throw new System.Exception("Not implemented");
        }
        public void GetFicheirosIDS(String bundleId) {
            throw new System.Exception("Not implemented");
        }
        public void RmFicheiroInfo(String Ficheiro, String bundleID) {
            throw new System.Exception("Not implemented");
        }
        public void SaveConnection(ConnectionInfo con) {
            throw new System.Exception("Not implemented");
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
                    conI = new ConnectionInfo(username, reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4));
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
        
        
        public void writeRegisto(string op_id, string op_type, string old_bundle_id, string new_bundle_id, string old_file_id, string new_file_id, string tag){
                
                //MUDAR ESTA CENA --- - - -  -  -  -> POR A POR A RECEBER UM OBJECTO

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
                SqlCommand query = new SqlCommand("INSERT INTO historico (op_id, op_type, old_bundle_id, new_bundle_id, old_file_id, new_file_id, tag, etiqueta) Values ('" + op_id + "', '" + op_type + "', '" + old_bundle_id + "', '" + new_bundle_id + "', '" + old_file_id + "', '" + new_file_id + "', '" + tag + "', getdate())", con);
                query.ExecuteNonQuery();
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
                reg.op_id = op_id;
                reg.op_type = reader.GetString(1);
                reg.old_bundle_id = reader.GetString(2);
                reg.new_bundle_id = reader.GetString(3);
                reg.old_file_id = reader.GetString(4);
                reg.new_file_id = reader.GetString(5);
                reg.tag = reader.GetString(6);
                reg.etiqueta = (DateTime)reader.GetDateTime(7);
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