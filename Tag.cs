using System;
namespace WeeboxSync {
    public class Tag : ICloneable {
        public string Name { get; set; }
        public string Path { get; set; }
        public string WeeId { get ; set; }

        public Tag() { Name = ""; Path = "";  }

        public Tag(string tag, string path, string WeeId) { this.Name = tag; this.Path = path; this.WeeId = WeeId; }
        public Tag(Tag t) {
            this.Name = t.Name;
            this.WeeId = WeeId;
            this.Path = t.Path; 
        }

       override public Boolean Equals(Object t) {
            if (t == null) return false;
            if (t == this) return true; 
            if (t.GetType() != this.GetType())  return false;
            Tag tag = (Tag) t;
            try {
                return (tag.Name.Equals(this.Name) && tag.Path.Equals(this.Path) && tag.WeeId.Equals(this.WeeId));
            }
            catch (Exception ) {
                return false; 
            }
        }

        public object  Clone(){
            return new Tag(this);
        }

    }
}
