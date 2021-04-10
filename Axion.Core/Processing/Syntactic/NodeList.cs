using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Traversal;

namespace Axion.Core.Processing.Syntactic {
    /// <summary>
    ///     A special implementation of List that can handle expressions.
    ///     It can automatically bind parent of items
    ///     to one defined on list construction,
    ///     and provide some other useful methods.
    /// </summary>
    public class NodeList<T> : IList<T> where T : Node {
        readonly Node? parent;

        public Node? Parent {
            get => parent;
            init {
                parent = value;
                if (parent == null) {
                    return;
                }

                foreach (var item in items) {
                    if (item != null) {
                        item.Parent = parent;
                    }
                }
            }
        }

        readonly IList<T> items;

        void IList<T>.RemoveAt(int index) {
            items.RemoveAt(index);
        }

        public T this[int i] {
            get => items[i];
            set {
                Parent?.Bind(value, this, i);
                items[i] = value;
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

        public NodeList(Node parent) {
            items  = new List<T>();
            Parent = parent;
        }

        public NodeList(Node parent, IEnumerable<T> collection) {
            items  = collection.ToList();
            Parent = parent;
        }

        public void Insert(int index, T item) {
            if (index == Count) {
                Add(item);
            }
            else {
                Parent?.Bind(item, this, index);
                items.Insert(index, item);
                for (var i = index; i < Count; i++) {
                    ((NodeListTreePath<T>) items[i].Path).IndexInList = i;
                }
            }
        }

        public NodeList<T> Add(T item) {
            Parent?.Bind(item, this, Count);
            items.Add(item);
            return this;
        }

        public NodeList<T> AddRange(IEnumerable<T> list) {
            foreach (T item in list) {
                items.Add(item);
            }
            return this;
        }

        public static NodeList<T> operator +(NodeList<T> list, T item) {
            return list.Add(item);
        }

        public int IndexOf(T item) {
            return items.IndexOf(item);
        }

        public bool Contains(T item) {
            return items.Contains(item);
        }

        public void Clear() {
            items.Clear();
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
        public void CopyTo(T[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (array.Rank > 1) {
                throw new ArgumentException(
                    "Only single dimensional arrays are supported for the requested action.",
                    nameof(array)
                );
            }

            if (array.Length - arrayIndex < Count) {
                throw new ArgumentException(
                    "Not enough elements after index in the destination array."
                );
            }

            for (var i = 0; i < Count; i++) {
                array.SetValue(this[i], i + arrayIndex);
            }
        }
    }
}
