using System;
using System.Collections;
using System.Collections.Generic;

namespace Axion.Core.Processing.Syntactic {
    public class NodeList<T> : IList<T> where T : AstNode {
        public readonly  AstNode Parent;
        private readonly List<T> items;

        void IList<T>.RemoveAt(int index) {
            items.RemoveAt(index);
        }

        public T this[int i] {
            get => items[i];
            set => items[i] = value;
        }

        public bool Remove(T item) {
            return items.Remove(item);
        }

        public int  Count      => items.Count;
        public bool IsReadOnly => false;

        public T First {
            get =>
                items.Count > 0
                    ? items[0]
                    : throw new IndexOutOfRangeException();
            set {
                if (items.Count > 0) {
                    items[0] = value;
                }
                else {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public T Last {
            get =>
                items.Count > 0
                    ? items[items.Count - 1]
                    : throw new IndexOutOfRangeException();
            set {
                if (items.Count > 0) {
                    items[items.Count - 1] = value;
                }
                else {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        internal NodeList(AstNode parent) {
            Parent = parent;
            items  = new List<T>();
        }

        internal NodeList(AstNode parent, IEnumerable<T> array) {
            Parent = parent;
            items  = new List<T>(array);
        }

        public NodeList<T> Insert(int index, T item) {
            item.Parent = Parent;
            items.Insert(index, item);
            return this;
        }

        public NodeList<T> Add(T item) {
            item.Parent = Parent;
            items.Add(item);
            return this;
        }

        public int IndexOf(T item) {
            return items.IndexOf(item);
        }

        public bool Contains(T item) {
            return items.Contains(item);
        }

        public NodeList<T> AddRange(IEnumerable<T> collection) {
            foreach (T syntaxTreeNode in collection) {
                syntaxTreeNode.Parent = Parent;
                items.Add(syntaxTreeNode);
            }

            return this;
        }

        public NodeList<T> RemoveAt(int index) {
            items.RemoveAt(index);
            return this;
        }

        public NodeList<T> Clear() {
            items.Clear();
            return this;
        }

        public IEnumerator<T> GetEnumerator() {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        [Obsolete]
        void IList<T>.Insert(int index, T item) {
            throw new NotSupportedException();
        }

        [Obsolete]
        void ICollection<T>.Add(T item) {
            throw new NotSupportedException();
        }

        [Obsolete]
        void ICollection<T>.Clear() {
            throw new NotSupportedException();
        }

        [Obsolete]
        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotSupportedException();
        }
    }
}