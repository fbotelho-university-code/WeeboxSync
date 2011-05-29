using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace WeeboxSync {
    /// <remarks/>
    public class DocType {
        public String id { get; set; }
        public String label { get; set; }
        public String description { get; set; }
        public bool deflt { get; set;  }
        public List<Field> fields { get; set; }
        public List<Field> tagFields { get; set;  }

        public class Field {
            public String id { get; set; }
            public String typeArgs { get; set; }
            public String type { get; set; }
            public String label { get; set; }
            public String description { get; set; }

        }

        public DocType(){
            id = label = description = "";
            deflt = false; 
            fields = new List<Field>();
        }

        public override string  ToString(){
            StringBuilder s = new StringBuilder();
            s.AppendLine("DocType");
            s.AppendLine("ID : " + id);
            s.AppendLine("Label: " + label);
            s.AppendLine("Description: " + description);
            s.AppendLine("Default: " + deflt);
            foreach (Field f in fields){
                s.AppendLine(String.Format("Id: {0} , typeArgs : {1}, type : {2} ,label : {3} , description{4}", f.id,
                                           f.typeArgs, f.type, f.label, f.description));
            }
            return s.ToString(); 
        }

        private static string _getKeepName(string s){
            return "{http://www.keep.pt/document/type}"  + s; 
        }

        public static DocType readAsXelement(System.Xml.Linq.XElement xroot) {
            DocType res = new DocType();
            XElement el;

            XAttribute at = xroot.Attribute("id"); 
            if (at == null) return null; 
            else res.id = at.Value; 

            
            el = xroot.Element(_getKeepName("label")); 

            if (el==null) res.label = ""; 
            else res.label = el.Value;

            el = xroot.Element(_getKeepName("description"));
            if (el==null) res.description = ""; 
            else res.description = el.Value;

            //TODO - what if fld.id is null ? 
            XElement list = xroot.Element(_getKeepName("fieldList"));
            if (list != null)
            foreach (XElement field in list.Elements()){
                //for each in field 
                Field fld = new Field();
                fld.id = (at = field.Attribute("id")) == null ? "" : at.Value;
                if (fld.id == null) continue;
                fld.type =  (at = field.Attribute("type")) == null ? "" : at.Value; 
                fld.typeArgs =  (at = field.Attribute("typeArgs")) == null ? "" : at.Value; 
                //TODO - atribuiçºao 
                fld.label = (el = field.Element(_getKeepName("label"))) == null ? "" : el.Value; 
                fld.description = (el = field.Element(_getKeepName("description"))) == null ? "" : el.Value; 
                res.fields.Add(fld);
                //create faster tag helper list 
                if (fld.type =="vocabulary" && fld.id != "" && fld.typeArgs != ""){
                    //we have a field wich indicates tags.
                    res.tagFields.Add(fld);
                }
            }
            return res; 
        }
    }
}
