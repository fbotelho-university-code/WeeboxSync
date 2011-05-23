using System;
namespace WeeboxSync {
    public class Utilizador {
        public Utilizador(string user, string pass) {
            // TODO: Complete member initialization
            this.user = user ;
            this.pass = pass;
        }

        public string user { get; set; }
        public string  pass { get; set; } 
    }
}
