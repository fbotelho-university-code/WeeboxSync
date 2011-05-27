using System;
using System.Collections.Generic;

namespace WeeboxSync {

    public class Scheme {
        public Tree<Tag> arvore;
        public Tree<Tag> arvoreByWeeboxIds; 
        public string id { get; set; }

        public Scheme(string id, Tag root){
            arvore = new Tree<Tag>(root, root.Path);
            arvoreByWeeboxIds = new Tree<Tag>(root, root.WeeId); 
            this.id = id;
        }

        public Scheme(Tree<Tag> arvore,Tree<Tag> arvoreByWeeboxIds, string origem, string id) {
            this.arvore = arvore;
            this.arvoreByWeeboxIds = arvoreByWeeboxIds; 
            this.id = id;
        }

        public Scheme(Scheme sc) {
            this.arvore = sc.arvore;
            this.arvore = sc.arvoreByWeeboxIds; 
            this.id = sc.id;
        }

        public Scheme Clone() { return new Scheme(this); }

        override public bool Equals(Object n) {
            if (n == null) return false;
            if (n == this) return true;
            if (n.GetType() != this.GetType()) return false;
            Scheme s = (Scheme )n;
            return (this.id.Equals(s.arvore) && this.arvore.Equals(s.arvore) && this.arvoreByWeeboxIds.Equals(s.arvoreByWeeboxIds));
        }

        
        public static bool containsWee(string id , List<Scheme> scheme){
            return Scheme.getTagByWeeIds(id, scheme) != null; 
        }
        
        public static bool containsLocal(string id , List<Scheme> scheme){
            return Scheme.getTagByWeeIds(id, scheme) != null; 
        }

        public static  Tag getTagByWeeIds(string id, List<Scheme> scheme ){
            foreach (Scheme s in scheme){
                Tag t = s.arvoreByWeeboxIds.find(id); 
                if (t != null) return t; 
            }
            return null; 
        }

        public static  Tag getTagByLocalds(string id, List<Scheme> scheme){
            foreach (Scheme s in scheme){
                Tag t = s.arvore.find(id); 
                if (t != null) return t; 
            }
            return null; 
        }

    }
}

