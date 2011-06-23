using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace WeeboxSync {
    public class DataBaseAbstraction {

        private static string ROOT_TAG = "rootScheme_trashFiller";
        //private static string connectionString = "Server=FABIIM-PC\\SQLEXPRESS;Database=master;Trusted_Connection=True;"; 
        private static string connectionString = "Data Source=(local);Integrated Security=True";
        //TODO - tocha - change return on error to ArgumentNullException()
        /**
         * retrieves the classification scheme of this weebox server instance
         */

        public List<Scheme> GetClassificationScheme() {
            List<Scheme> lista = new List<Scheme>();
            SqlConnection con = new SqlConnection(connectionString);
            try {
                con.Open();
                //fetch the parents of each scheme
                List<Scheme> parentList = new List<Scheme>();
                SqlCommand query = new SqlCommand(string.Format(
                    "select child_name, child_weeID, child_path, parent_path from plano_classificacao where parent_weeID = '{0}'", ROOT_TAG), con);
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    Tag tempTag = new Tag(reader.GetString(0), reader.GetString(2), reader.GetString(1));
                    Scheme tempScheme = new Scheme(reader.GetString(3), tempTag);
                    parentList.Add(tempScheme);
                }
                reader.Close();
                foreach (Scheme parent in parentList) {
                    lista.Add(GetChildTags(parent.arvore.getRoot(), parent, con));
                }
                con.Close();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            if (lista.Count == 0)
                lista = null;
            return lista;
        }

        private Scheme GetChildTags(Tag parent, Scheme scheme, SqlConnection con) {
            SqlCommand query = new SqlCommand(string.Format(
                       "select child_name, child_weeID, child_path from plano_classificacao where parent_weeID = '{0}'", parent.WeeId), con);
            SqlDataReader reader = query.ExecuteReader();
            List<Tag> childs = new List<Tag>();
            while (reader.Read()) {
                Tag tempTag = new Tag(reader.GetString(0), reader.GetString(2), reader.GetString(1));
                childs.Add(tempTag);
                scheme.arvore.add(tempTag, parent.Path, tempTag.Path);
                scheme.arvoreByWeeboxIds.add(tempTag, parent.WeeId, tempTag.WeeId);
            }
            reader.Close();
            foreach (var child in childs) {
                scheme = GetChildTags(child, scheme, con);
            }
            return scheme;
        }
        /**
         * stores the classification scheme of this weebox server instance
         * ::Checked::
         */
        public void SaveClassificationScheme(IEnumerable<Scheme> lista) {
            if (lista == null)
                return;
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query;
            foreach (Scheme scheme in lista) {
                Tag root = scheme.arvore.getRoot();
                //save root
                string[] values = { "rootName", ROOT_TAG, scheme.id, root.Name, root.WeeId, root.Path };
                query = new SqlCommand(string.Format(
                    "INSERT INTO plano_classificacao (parent_name, parent_weeID, parent_path, child_name, child_weeID, child_path) " +
                    "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", values), con);
                query.ExecuteNonQuery();
                SaveTag(root, scheme, con);
            }
            con.Close();
        }

        private void SaveTag(Tag t, Scheme scheme, SqlConnection con) {
            foreach (Tag tFilho in scheme.arvore.findChilds(t.Path)) {
                SqlCommand query = new SqlCommand(
                    "INSERT INTO plano_classificacao (parent_name, parent_weeID, parent_path, child_name, child_weeID, child_path) VALUES ('" +
                    t.Name + "', '" + t.WeeId + "', '" + t.Path + "', '" + tFilho.Name + "', '" + tFilho.WeeId + "', '" + tFilho.Path + "')", con);
                query.ExecuteNonQuery();
                SaveTag(tFilho, scheme, con);
            }
        }

        /**
         * deletes the specified bundle from the database
         * ::Checked::
         */

        public void DeleteBundle(String bundleId){

            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand(string.Format(
                "DELETE FROM tags WHERE BundleID = '{0}'", bundleId), con);
            query.ExecuteNonQuery();
            query = new SqlCommand(string.Format(
                "DELETE FROM ficheiro WHERE BundleID = '{0}'", bundleId), con);
            query.ExecuteNonQuery();
            query = new SqlCommand(string.Format(
                "DELETE FROM metadata WHERE BundleID = '{0}'", bundleId), con);
            query.ExecuteNonQuery();
            query = new SqlCommand(string.Format(
                "DELETE FROM last_updated_versions WHERE bundle_version_ID = '{0}'", bundleId), con);
            query.ExecuteNonQuery();
            query = new SqlCommand(string.Format(
                "delete from bundle where ID = '{0}'", bundleId), con);
            query.ExecuteNonQuery();
            con.Close();
        }

        /**
         * retrieves all the bundles stored in the database
         * ::Checked::
         */
        public List<Bundle> GetAllBundles() {//todas as tabelas
            Bundle bundle = new Bundle();
            List<Bundle> lista = new List<Bundle>();
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand("SELECT * FROM bundle", con);
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read()) {
                bundle = GetBundle(reader.GetString(0));
                lista.Add(bundle);
            }
            con.Close();
            return lista;
        }

        ///<summary>
        /// retrieves one bundle stored in the database
        /// ::Checked::
        ///</summary>
        public Bundle GetBundle(string id) {
            if (string.IsNullOrEmpty(id))
                return null;
            Bundle bundle = new Bundle();
            
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand(string.Format("SELECT * FROM bundle WHERE id = '{0}'", id), con);
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read()) {
                bundle.localId = reader.GetString(0);
                bundle.weeId = reader.GetString(1);
            }
            reader.Close();
            query = new SqlCommand(string.Format("SELECT * FROM ficheiro where bundleID = '{0}'", id), con);
            reader = query.ExecuteReader();
            var list = new List<Ficheiro>();
            while (reader.Read()) {
                Ficheiro f = new Ficheiro(
                    //path                  bundleID                    md5
                    reader.GetString(2), reader.GetString(1), reader.GetString(0));
                list.Add(f);
            }
            bundle.filesPath = list;
            reader.Close();
            query = new SqlCommand(string.Format("SELECT * FROM tags where bundleID = '{0}'", id), con);
            reader = query.ExecuteReader();
            while (reader.Read()) {
                bundle.weeTags.Add(reader.GetString(1));
            }
            reader.Close();
            query = new SqlCommand(string.Format("SELECT * FROM metadata where bundleID = '{0}'", id), con);
            reader = query.ExecuteReader();
            while (reader.Read()) {
                bundle.meta.keyValueData.Add(reader.GetString(1), reader.GetString(2));
            }
            reader.Close();

            con.Close();
            return bundle;
        }
        /**
         * stores a bundle into the database.
         * any old info associated with that bundle is cleared.
         * ::Checked::
         */
        public void SaveBundle(Bundle bundle) {
            if (bundle == null)
                return;
            
            SqlConnection con = new SqlConnection(connectionString);

            con.Open();
            SqlCommand query = new SqlCommand(string.Format(
                "INSERT INTO bundle (id, version_ID) VALUES ('{0}', '{1}')",
                bundle.localId, bundle.weeId), con);
            query.ExecuteNonQuery();
            // clear old data from last_updated_versions
            query = new SqlCommand(string.Format(
                "delete from last_updated_versions where bundle_version_ID = '{0}'", bundle.weeId), con);
            query.ExecuteNonQuery();
            foreach (Ficheiro f in bundle.filesPath) {
                // insert last_updated_versions
                query = new SqlCommand(string.Format(
                    "INSERT INTO last_updated_versions (bundle_version_ID, file_ID, nome_ficheiro) VALUES ('{0}', '{1}', '{2}')",
                    bundle.localId, bundle.weeId, f.path), con);
                query.ExecuteNonQuery();
            }
            //clear old files
            query = new SqlCommand(string.Format(
                "delete from ficheiro where bundleID = '{0}'", bundle.weeId), con);
            query.ExecuteNonQuery();
            foreach (Ficheiro f in bundle.filesPath) {
                // insert new files
                query = new SqlCommand(string.Format(
                    "INSERT INTO ficheiro (id, BundleID, nome_ficheiro) VALUES ('{0}', '{1}', '{2}')",
                    f.md5, bundle.localId, f.path), con);
                query.ExecuteNonQuery();
            }
            //clear existing metadata
            query = new SqlCommand(string.Format(
                "delete from metadata where BundleID = '{0}'", bundle.weeId), con);
            query.ExecuteNonQuery();
/*            foreach (KeyValuePair<String, String> kvp in bundle.meta.keyValueData) {
                // insert metadata
                String v1 = kvp.Key;
                String v2 = kvp.Value;
                query = new SqlCommand(string.Format(
                    "INSERT INTO metadata (BundleID, field_name, field_data) VALUES ('{0}', '{1}', '{2}')",
                    bundle.localId, v1, v2), con);
                query.ExecuteNonQuery();
            }
 */
            //clear existing tags
            query = new SqlCommand(string.Format(
                "delete from tags where BundleID = '{0}'", bundle.weeId), con);
            query.ExecuteNonQuery();
            foreach (String tag in bundle.weeTags) {
                // insert new tags
                query =
                    new SqlCommand(
                        string.Format("INSERT INTO tags (BundleID, tag) VALUES ('{0}', '{1}')", bundle.localId, tag),
                        con);
                query.ExecuteNonQuery();
            }
            con.Close();
        }

        /**
            * retrieves the files associated with a bundle
            * ::Checked::
            */
        public List<Ficheiro> GetFicheirosIDS(String bundleId) {
            if (string.IsNullOrEmpty(bundleId))
                return null;
            List<Ficheiro> lista = new List<Ficheiro>();
            
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand(string.Format("select * from ficheiro where bundleID = '{0}'", bundleId), con);
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read()) {
                Ficheiro file = new Ficheiro(reader.GetString(2), reader.GetString(1), reader.GetString(0));
                lista.Add(file);
            }
            reader.Close();
            con.Close();
            return lista;
        }
        /**
            * stores the file information
         * ::Checked::
            */

        public void UpdateFicheiroInfo(Ficheiro file) {
            DeleteFicheiroInfo(file.md5, file.name);
            SaveFicheiroInfo(file);
        }

        public void UpdateBundle(Bundle b) {
            DeleteBundle(b.localId);
            SaveBundle(b);
        }

        public void SaveFicheiroInfo(Ficheiro ficheiro) {
            if (ficheiro == null)
                return;
            
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand(string.Format(
                "INSERT INTO ficheiro (id, bundleID, nome_ficheiro) Values ('{0}', '{1}', '{2}')",
                ficheiro.md5, ficheiro.bundleId, ficheiro.path), con);
            query.ExecuteNonQuery();
            con.Close();
        }

        /**
            * removes the information associated with the file
         * ::Checked::
            */

        public void DeleteFicheiroInfo(String ficheiro, String bundleID) {
            if (string.IsNullOrEmpty(ficheiro) || string.IsNullOrEmpty(bundleID))
                return;
            
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand(string.Format(
                "delete from ficheiro where id = '{0}' and bundleID = '{1}'",
                ficheiro, bundleID), con);
            query.ExecuteNonQuery();
            con.Close();
        }

        /**
            * stores the connection information associated with this weebox instance
         * ::Checked::
            */
        public void SaveConnectionInfo(ConnectionInfo conI) {
            if (conI == null || conI.user == null || conI.address == null)
                return;
            
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            //check if data is set
            bool set;
            SqlCommand query = new SqlCommand(string.Format("select * from utilizador where username = '{0}'", conI.user.user), con);
            SqlDataReader reader = query.ExecuteReader();
            set = reader.Read();
            reader.Close();
            //insert or update values
            string proxy;
            proxy = conI.proxy != null ? conI.proxy.ToString() : string.Empty;

            string[] par = { conI.user.user, conI.user.pass, conI.address.ToString (), proxy,
                             conI.useProxy.ToString (), conI.serial_generator.ToString()};
            if (set) {
                //operation : update
                query = new SqlCommand(string.Format(
                    "update utilizador set " +
                    "username='{0}',password='{1}', server='{2}', proxy='{3}', useProxy='{4}', serial_generator ='{5}'", par), con);
            }
            else {
                //operation : insert
                query = new SqlCommand(string.Format(
                    "insert into utilizador (username, password, server, proxy, useProxy, serial_generator) " +
                    "values ( '{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", par), con);
            }
            query.ExecuteNonQuery();
            con.Close();
        }

        /**
            * retrieves the connection information associated with this weebox instance
         * ::Checked::
            */
        public ConnectionInfo GetConnectionInfo(String username) {
            if (string.IsNullOrEmpty(username))
                return null;
            ConnectionInfo conInfo = null;
            
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand(string.Format(
                "SELECT username, password, server, proxy, serial_generator FROM utilizador WHERE username = '{0}'", username), con);
            SqlDataReader reader = query.ExecuteReader();
            if (reader.Read()) { //only the first occurrence
                Utilizador user = new Utilizador(reader.GetString(0), reader.GetString(1));
                string server = reader.GetString(2);
                string proxy = reader.GetString(3);
                if (!(string.IsNullOrEmpty(proxy) || proxy.Equals("null"))) {
                    //there is a proxy to save
                    conInfo = new ConnectionInfo(user, server, proxy);
                }
                else {
                    //no proxy
                    conInfo = new ConnectionInfo(user, server);
                }
                int defaultInt = reader.GetInt32 (4);
                conInfo.serial_generator = defaultInt;
            }
            reader.Close();
            con.Close();
            return conInfo;
        }

        /**
            * stores an operation record in history
            */
        public void SaveRegisto(Registo reg) {
            if (reg == null)
                return;
            
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand(string.Format(
                "INSERT INTO historico (op_id, op_type, old_bundle_id, new_bundle_id, old_file_id, new_file_id, tag, etiqueta) " +
                "Values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', getdate())",
                reg.op_id, reg.op_type, reg.old_bundle_id, reg.new_bundle_id, reg.old_file_id, reg.new_file_id, reg.tag), con);
            query.ExecuteNonQuery();

            con.Close();
        }

        /**
            * retrieves an operation record from history
            */
        public Registo GetRegisto(string op_id) {
            if (string.IsNullOrEmpty(op_id))
                return null;
            Registo reg = new Registo();
            
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand(string.Format("SELECT * FROM historico WHERE op_id = '{0}'", op_id), con);
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read()) {
                reg = new Registo(op_id,
                                  reader.GetDateTime(7), reader.GetString(1), reader.GetString(2),
                                  reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6));
            }
            reader.Close();
            con.Close();
            return reg;
        }

        public void deleteALl(){
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand("DELETE FROM FICHEIRO", con);
            query.ExecuteNonQuery(); 
             query = new SqlCommand("DELETE FROM metadata",con);
                        query.ExecuteNonQuery(); 
             query = new SqlCommand("DELETE FROM tags",con);
                      query.ExecuteNonQuery(); 
             query = new SqlCommand("DELETE FROM plano_classificacao",con);
                        query.ExecuteNonQuery(); 
             query = new SqlCommand("DELETE FROM last_updated_versions",con);
                        query.ExecuteNonQuery(); 
                         query = new SqlCommand("DELETE FROM bundle",con);

                        query.ExecuteNonQuery();
            query = new SqlCommand("DELETE FROM utilizador",con); 
                        query.ExecuteNonQuery();
            query = new SqlCommand("DELETE FROM historico",con); 
            con.Close();

        }

        public void UpdateWeeId(string localId, string bundleTransformedId){
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand query = new SqlCommand(String.Format("update bundle SET VERSION_ID = '{0}' WHERE ID ='{1}'", bundleTransformedId,localId),  con);
            query.ExecuteNonQuery();
            con.Close(); 
        }
    }
}
