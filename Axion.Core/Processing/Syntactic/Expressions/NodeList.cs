using System;
using System.Collections;
using System.Collections.Generic;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class NodeList<T> : IList<T> where T : Expr {
        public Expr Parent { get; }
        private readonly List<T> items;

        void IList<T>.RemoveAt(int index) {
            items.RemoveAt(index);
        }

        public T this[int i] {
            get => items[i];
            set {
                value.Parent = Parent;
                value.Path   = new NodeListTreePath<T>(this, i);
                items[i]     = value;
            }
        }

        public bool Remove(T item) {
            return items.Remove(item);
        }

        public int Count => items.Count;
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
                    ? items[^1]
                    : throw new IndexOutOfRangeException();
            set {
                if (items.Count > 0) {
                    items[^1] = value;
                }
                else {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        internal NodeList(Expr parent) {
            Parent = parent;
            items  = new List<T>();
        }

        internal NodeList(Expr parent, IEnumerable<T> array) {
            Parent = parent;
            items  = new List<T>(array);
        }

        public void Insert(int index, T item) {
            item.Parent = Parent;
            item.Path   = new NodeListTreePath<T>(this, index);
            if (index == Count) {
                items.Add(item);
            }
            else {
                items.Insert(index, item);
                for (int i = index; i < Count; i++) {
                    ((NodeListTreePath<T>) items[i].Path).IndexInList = i;
                }
            }
        }

        public void Add(T item) {
            item.Path   = new NodeListTreePath<T>(this, Count);
            item.Parent = Parent;
            items.Add(item);
        }

        public int IndexOf(T item) {
            return items.IndexOf(item);
        }

        public bool Contains(T item) {
            return items.Contains(item);
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
            Add(item);
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