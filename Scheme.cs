using System;
namespace WeeboxSync {

    public class Scheme {
        public Tree<Tag> arvore;
        public Tree<Tag> arvoreByWeeboxIds; 
        public string id { get; set; }

        public Scheme(string origem, string id, Tag root){
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
    }
}

