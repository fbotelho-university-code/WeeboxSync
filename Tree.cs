using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeeboxSync {
    public class Tree<T> {
        public Node<T> root;

        public void print() {
            root.printNode(0); 
        }

        public void setRoot(T value , string key){
            root = new Node<T>(); 
            root.value = value; 
            root.key = key; 
        }

        public T getRoot() { return root.value; }

        public Tree(T value, string key) {
            root= new Node<T>();
            root.value = value;
            root.key = key;
            root._parent = null;
        }



        public void add(T value, string parentKey, string key) {
            root.Add(value, parentKey, key);
        }


        public bool has(string key) { return (!root.find(key).Equals(default(T))); }

        /**
         * Returns default(T) if not found 
         */
        public T find(string key) {
            return root.find(key);
        }

        public T findParent(string key) {
            return root.findParent(key);
        }

        /**
         * Null if none
         */

        public IEnumerable<T> findChilds(string key) {
            return root.findChilds(key);
        }

        override public bool Equals(Object n) {
            if (n == null) return false;
            if (n == this) return true;
            if (n.GetType() != this.GetType()) return false;
            Tree<T> s = (Tree<T>)n;
            return (this.root.Equals(s.root));
        }
    }


    public class Node<T> {
        public void printNode(int level) {
            for (int i =0 ; i < level *3 ; i++){
                Console.Write(' '); 
            }
            Console.WriteLine(this.key);
            foreach (Node<T> t in _children) {
                t.printNode(level + 1); 
            }
        }
        public Node<T> _parent;
        public T value;
        public string key;
        private List<Node<T>> _children;

        private bool _testaFilhos(Node<T> n, List<Node<T>> s) {
            foreach (Node<T> filho in n._children) {
                bool cond = false;
                foreach (Node<T> filho_do_outro in s) {
                    if (filho.Equals(filho_do_outro)) {
                        cond = true;
                        break;
                    }
                    if (cond == false) return false;
                }
            }
            return true;
        }

        public override bool Equals(Object n) {
            if (n == null) return false;
            if (n == this) return true;
            if (n.GetType() != this.GetType()) return false;
            Node<T> s = (Node<T>)n;
            if (_parent == null) {
                if (s._parent != null) return false;
                else return value.Equals(s.value) && key.Equals(s.key) && _testaFilhos(this, s._children);
            }
            return (_parent.Equals(s.value) && value.Equals(s.value) && key.Equals(s.key) && _testaFilhos(this, s._children));
        }


        public List<T> findChilds(string k) {
            if (this.key.Equals(k)) {
                List<T> lista = new List<T>();
                foreach (Node<T> t in _children) {
                    lista.Add(t.value);
                }
                return lista;
            }
            foreach (Node<T> t in _children) {
                List<T> l = t.findChilds(k);
                if (t != null) return l;
            }
            return null;
        }

        /// <summary>
        /// returns null in case of Tags being the T"/>"/>
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public T find(string k) {
            if (this.key.Equals(k)) return this.value;

            foreach (Node<T> nd in _children) {
                T back= nd.find(k);
                if (back != null) return back; 
            }
            return default(T);
        }

        public T findParent(string k) {
            foreach (Node<T> nd in _children) {
                if (nd.key == k) return this.value;
            }
            foreach (Node<T> nd in _children) {
                T back;
                if (!(back = nd.find(k)).Equals(default(T))) return back;
            }
            return default(T);
        }

        public IEnumerable<T> Children {
            get {
                List<T> nodos = new List<T>();
                foreach (Node<T> value in _children) {
                    nodos.Add(value.value);
                }
                return nodos;
            }
        }

        public Node<T> Parent {
            get { return _parent; }
        }

        public Node() {
            _children = new List<Node<T>>();
        }

        public int Add(T value, string parentKey, string key) {
            if (this.key.Equals(parentKey)) {
                Node<T> child = new Node<T>();
                child.key = key;
                child.value = value;
                _children.Add(child);
                child.SetParent(this);
                return 1;
            }
            foreach (Node<T> nodo in _children) {
                if (nodo.Add(value, parentKey, key) == 1) return 1;
            }
            return 0; // not found 
        }

        private void SetParent(Node<T> parent) {
            _parent = parent;
        }
    }
}


