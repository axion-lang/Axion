using System;
using System.Collections;
using System.Collections.Generic;

namespace Axion.Core.Utils {
    /// <inheritdoc />
    /// <summary>
    /// This is a dictionary guaranteed to have only one of each value and key. 
    /// It may be searched either by T1 or by T2, giving a unique answer because it is 1 to 1.
    /// </summary>
    /// <typeparam name="T1">The type of the "key"</typeparam>
    /// <typeparam name="T2">The type of the "value"</typeparam>
    public class Map<T1, T2> : IEnumerable<KeyValuePair<T1, T2>> {
        public readonly Dictionary<T1, T2> Forward =
            new Dictionary<T1, T2>();

        public Indexer<T1, T2> ForwardI { get; }

        public readonly Dictionary<T2, T1> Reverse =
            new Dictionary<T2, T1>();

        public Indexer<T2, T1> ReverseI { get; }

        public Map() {
            ForwardI = new Indexer<T1, T2>(Forward);
            ReverseI = new Indexer<T2, T1>(Reverse);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() {
            return Forward.GetEnumerator();
        }

        public class Indexer<T3, T4> {
            private readonly Dictionary<T3, T4> dictionary;

            public Indexer(Dictionary<T3, T4> dictionary) {
                this.dictionary = dictionary;
            }

            public T4 this[T3 index] {
                get => dictionary[index];
                set => dictionary[index] = value;
            }

            public bool Contains(T3 key) {
                return dictionary.ContainsKey(key);
            }
        }

        #region Exception throwing methods

        /// <summary>
        /// Tries to add the pair to the dictionary.
        /// Throws an exception if either element is already in the dictionary
        /// </summary>
        public void Add(T1 first, T2 second) {
            if (Forward.ContainsKey(first) || Reverse.ContainsKey(second)) {
                throw new ArgumentException("Duplicate first or second");
            }

            Forward.Add(first, second);
            Reverse.Add(second, first);
        }

        /// <summary>
        /// Find the T2 corresponding to the T1 first
        /// Throws an exception if first is not in the dictionary.
        /// </summary>
        /// <param name="first">the key to search for</param>
        /// <returns>the value corresponding to first</returns>
        public T2 GetByFirst(T1 first) {
            if (!Forward.TryGetValue(first, out T2 second)) {
                throw new ArgumentException("first");
            }

            return second;
        }

        /// <summary>
        /// Find the T1 corresponding to the Second second.
        /// Throws an exception if second is not in the dictionary.
        /// </summary>
        /// <param name="second">the key to search for</param>
        /// <returns>the value corresponding to second</returns>
        public T1 GetBySecond(T2 second) {
            if (!Reverse.TryGetValue(second, out T1 first)) {
                throw new ArgumentException("second");
            }

            return first;
        }

        /// <summary>
        /// Remove the record containing first.
        /// If first is not in the dictionary, throws an Exception.
        /// </summary>
        /// <param name="first">the key of the record to delete</param>
        public void RemoveByFirst(T1 first) {
            if (!Forward.TryGetValue(first, out T2 second)) {
                throw new ArgumentException("first");
            }

            Forward.Remove(first);
            Reverse.Remove(second);
        }

        /// <summary>
        /// Remove the record containing second.
        /// If second is not in the dictionary, throws an Exception.
        /// </summary>
        /// <param name="second">the key of the record to delete</param>
        public void RemoveBySecond(T2 second) {
            if (!Reverse.TryGetValue(second, out T1 first)) {
                throw new ArgumentException("second");
            }

            Reverse.Remove(second);
            Forward.Remove(first);
        }

        #endregion

        #region Try methods

        /// <summary>
        /// Tries to add the pair to the dictionary.
        /// Returns false if either element is already in the dictionary        
        /// </summary>
        /// <returns>true if successfully added, false if either element are already in the dictionary</returns>
        public bool TryAdd(T1 first, T2 second) {
            if (Forward.ContainsKey(first) || Reverse.ContainsKey(second)) {
                return false;
            }

            Forward.Add(first, second);
            Reverse.Add(second, first);
            return true;
        }

        /// <summary>
        /// Find the T2 corresponding to the T1 first.
        /// Returns false if first is not in the dictionary.
        /// </summary>
        /// <param name="first">the key to search for</param>
        /// <param name="second">the corresponding value</param>
        /// <returns>true if first is in the dictionary, false otherwise</returns>
        public bool TryGetByFirst(T1 first, out T2 second) {
            return Forward.TryGetValue(first, out second);
        }

        /// <summary>
        /// Find the T1 corresponding to the T2 second.
        /// Returns false if second is not in the dictionary.
        /// </summary>
        /// <param name="second">the key to search for</param>
        /// <param name="first">the corresponding value</param>
        /// <returns>true if second is in the dictionary, false otherwise</returns>
        public bool TryGetBySecond(T2 second, out T1 first) {
            return Reverse.TryGetValue(second, out first);
        }

        /// <summary>
        /// Remove the record containing first, if there is one.
        /// </summary>
        /// <returns> If first is not in the dictionary, returns false, otherwise true</returns>
        public bool TryRemoveByFirst(T1 first) {
            if (!Forward.TryGetValue(first, out T2 second)) {
                return false;
            }

            Forward.Remove(first);
            Reverse.Remove(second);
            return true;
        }

        /// <summary>
        /// Remove the record containing second, if there is one.
        /// </summary>
        /// <returns> If second is not in the dictionary, returns false, otherwise true</returns>
        public bool TryRemoveBySecond(T2 second) {
            if (!Reverse.TryGetValue(second, out T1 first)) {
                return false;
            }

            Reverse.Remove(second);
            Forward.Remove(first);
            return true;
        }

        #endregion

        /// <summary>
        /// The number of pairs stored in the dictionary
        /// </summary>
        public int Count => Forward.Count;

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        public void Clear() {
            Forward.Clear();
            Reverse.Clear();
        }
    }
}