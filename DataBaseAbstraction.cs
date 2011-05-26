using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Collections;

namespace WeeboxSync {
    public class DataBaseAbstraction {

        public void GetClassificationScheme() {
            throw new System.Exception("Not implemented");
        }
        public void SaveClassificationScheme(Scheme scheme) {
            throw new System.Exception("Not implemented");
        }
        public void UpdateClassificationScheme(Scheme scheme) {
            throw new System.Exception("Not implemented");
        }
        public void DeleteBundle(String bundleId)
        {
            throw new System.Exception("Not implemented");
        }






        public List<Bundle> GetAllBundles()
        {//todas as tabelas
            Bundle bundle = new Bundle();
            List<Bundle> lista = new List<Bundle>();
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                SqlCommand query = new SqlCommand("SELECT * FROM bundle", con);
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read()){
                    bundle = getBundle(reader.GetString(0));
                    lista.Add(bundle);
                }
                con.Open();
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

        public Bundle getBundle(string id)
        {
            Bundle bundle = new Bundle();
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                SqlCommand query = new SqlCommand("SELECT * FROM bundle WHERE id = '" + id + "'", con);
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    bundle.localId = reader.GetString(0);
                    bundle.weeId = reader.GetString(1);
                }
                query = new SqlCommand("SELECT nome_ficheiro FROM last_updated_bundles WHERE bundle_version_ID = '" + id + "'", con);
                reader = query.ExecuteReader();
                while (reader.Read())
                {
                    //List<>
                    //bundle.filesPath -> adicionar path aos ficheiros
                }
                query = new SqlCommand("SELECT * FROM ficheiro where bundleID = '"+id+"'", con);
                reader = query.ExecuteReader();
                while (reader.Read())
                {
                    //bundle.filesPath -> adicionar md5 e bundleID aos ficheiros
                }
                query = new SqlCommand("SELECT * FROM tag where bundleID = '" + id + "'", con);
                reader = query.ExecuteReader();
                while (reader.Read())
                {
                    bundle.weeTags.Add(reader.GetString(1)); // so adiciono 1???????
                }
                query = new SqlCommand("SELECT * FROM metadata where bundleID = '" + id + "'", con);
                reader = query.ExecuteReader();
                while (reader.Read())
                {
                    bundle.meta.keyValueData.Add(reader.GetString(1), reader.GetString(2)); // so adiciono 1????
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
            return bundle;
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
                    query = new SqlCommand("INSERT INTO last_updated_bundles (bundle_version_ID, file_ID, nome_ficheiro) VALUES ('" + bundle.localId + "', '" + bundle.weeId + "', '" + f.path + "')", con);
                    query.ExecuteNonQuery();
                }
                foreach (Ficheiro f2 in bundle.filesPath)
                {
                    query = new SqlCommand("INSERT INTO ficheiro (id, BundleID, nome_ficheiro) VALUES ('" + f2.md5 + "', '" + bundle.localId + "', '" + f2.path + "')", con);
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

        public IList<Ficheiro> GetFicheirosIDS(String bundleId) {//todos os ficheiros k tem o bundleID -------->>> e o id-> outro primary key. nao interessa????
            Ficheiro file = new Ficheiro();
            List<Ficheiro> lista = new List<Ficheiro>();
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
        
        public void RmFicheiroInfo(String Ficheiro, String bundleID) {
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

        public void SaveConnectionInfo(ConnectionInfo conI) {
            string ConnectionString = "Data Source=(local);Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
               // SqlCommand query = new SqlCommand("INSERT INTO utilizador (username, password, server_address, server_port, proxy) VALUES ('" + conI.user.user + "', '" + conI.user.pass + "', '" + conI.address.Scheme + "', '" + conI.address.Port + "', '" + conI.proxy.Scheme + "')", con);
//                query.ExecuteNonQuery();
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
                    //conI = new ConnectionInfo(username, reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt64(5), reader.GetString(6));
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
                SqlCommand query = new SqlCommand("SELECT * FROM historico WHERE op_id = '" + op_id + "'", con);
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    reg = new Registo(op_id, (DateTime)reader.GetDateTime(7), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6));
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
            return reg;
    }
}
}