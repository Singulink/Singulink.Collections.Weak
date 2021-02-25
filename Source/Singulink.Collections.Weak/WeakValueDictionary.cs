﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Singulink.Collections
{
    /// <summary>
    /// Represents a collection of keys and weakly referenced values. If this collection is accessed from multiple threads, all accesses must be synchronized
    /// with a lock.
    /// </summary>
    /// <remarks>
    /// <para>Internal entries for garbage collected values are removed as they are encountered, i.e. if a key lookup is performed on a garbage collected value
    /// or if all the keys/values are enumerated. You can perform a full clean by calling the <see cref="Clean"/> method or configure automatic cleaning after
    /// a set number of add operations by setting the <see cref="AutoCleanAddCount"/> property.</para>
    /// </remarks>
    public class WeakValueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : class
    {
        private readonly Dictionary<TKey, WeakReference<TValue>> _entryLookup;
        private int _autoCleanAddCount;
        private int _addCountSinceLastClean;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakValueDictionary{TKey, TValue}"/> class.
        /// </summary>
        public WeakValueDictionary() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakValueDictionary{TKey, TValue}"/> class using the specified key equality comparer.
        /// </summary>
        public WeakValueDictionary(IEqualityComparer<TKey>? comparer)
        {
            _entryLookup = new(comparer);
        }

        /// <summary>
        /// Gets or sets the number of add (or indexer set) operations that automatically triggers the <see cref="Clean"/> method to run. Default value is
        /// <see langword="null"/> which indicates that automatic cleaning is not performed.
        /// </summary>
        public int? AutoCleanAddCount {
            get => _autoCleanAddCount == 0 ? null : _autoCleanAddCount;
            set {
                if (_autoCleanAddCount < 1)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _autoCleanAddCount = value.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Gets the number of add (or indexer set) operations that have been performed since the last cleaning.
        /// </summary>
        public int AddCountSinceLastClean => _addCountSinceLastClean;

        /// <summary>
        /// Gets or sets a value indicating whether to automatically call <see cref="TrimExcess"/> whenever <see cref="Clean"/> is called. Default value is
        /// <see langword="false"/>.
        /// </summary>
        public bool TrimExcessDuringClean { get; set; }

        /// <summary>
        /// Gets the keys in the dictionary.
        /// </summary>
        public IEnumerable<TKey> Keys => this.Select(kvp => kvp.Key);

        /// <summary>
        /// Gets the values in the dictionary.
        /// </summary>
        public IEnumerable<TValue> Values => this.Select(kvp => kvp.Value);

        /// <summary>
        /// Gets the approximate number of key/value pairs in this dictionary. This value may be considerably off if there are lots of garbage collected values
        /// that still have internal entries in the dictionary.
        /// </summary>
        /// <remarks>
        /// <para>This count may not be accurate if values have been collected since the last clean or enumeration. You can call <see cref="Clean"/> to force a
        /// full sweep before reading the count to get a more accurate value. If you require an accurate count then you should copy the values into a new
        /// strongly referenced collection (i.e. a list) so that they can't be garbage collected and use that collection to obtain the count and access the
        /// values.</para>
        /// </remarks>
        public int UnreliableCount => _entryLookup.Count;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        public TValue this[TKey key] {
            get {
                if (!TryGetValue(key, out var value))
                    throw new KeyNotFoundException();

                return value;
            }
            set {
                _entryLookup[key] = new WeakReference<TValue>(value);
                OnAdded();
            }
        }

        /// <summary>
        /// Attempts to add the specified key and value to the dictionary.
        /// </summary>
        public bool TryAdd(TKey key, TValue value)
        {
            if (_entryLookup.TryAdd(key, new WeakReference<TValue>(value))) {
                OnAdded();
                return true;
            }

            if (!_entryLookup.TryGetValue(key, out var entry) || entry.TryGetTarget(out _))
                return false;

            entry.SetTarget(value);
            OnAdded();

            return true;
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <exception cref="ArgumentException">The specified key already exists in the dictionary.</exception>
        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
                throw new ArgumentException("Specified key already exists.", nameof(key));
        }

        /// <summary>
        /// Attempts to remove the value with the specified key from the dictionary.
        /// </summary>
        public bool Remove(TKey key)
        {
            if (_entryLookup.Remove(key, out var entry) && entry.TryGetTarget(out _))
                return true;

            return false;
        }

        /// <summary>
        /// Attempts to remove the value with the specified key from the dictionary.
        /// </summary>
        public bool Remove(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            if (_entryLookup.Remove(key, out var entry) && entry.TryGetTarget(out value))
                return true;

            value = null;
            return false;
        }

        /// <summary>
        /// Attempts to remove the entry with the given key and value from the dictionary using the specified equality comparer for the value type.
        /// </summary>
        public bool Remove(TKey key, TValue value, IEqualityComparer<TValue>? comparer = null)
        {
            if (_entryLookup.TryGetValue(key, out var entry)) {
                if (entry.TryGetTarget(out var current)) {
                    if ((comparer ?? EqualityComparer<TValue>.Default).Equals(value, current)) {
                        _entryLookup.Remove(key);
                        return true;
                    }
                }
                else {
                    _entryLookup.Remove(key);
                }
            }

            return false;
        }

        /// <summary>
        /// Indictes whether the dictionary contains the specified key/value pair using the specified value comparer.
        /// </summary>
        public bool Contains(KeyValuePair<TKey, TValue> kvp, IEqualityComparer<TValue>? comparer = null) => Contains(kvp.Key, kvp.Value, comparer);

        /// <summary>
        /// Indictes whether the dictionary contains the key and value using the specified value comparer.
        /// </summary>
        public bool Contains(TKey key, TValue value, IEqualityComparer<TValue>? comparer = null)
        {
            if (_entryLookup.TryGetValue(key, out var entry) && entry.TryGetTarget(out var current))
                return (comparer ?? EqualityComparer<TValue>.Default).Equals(value, current);

            return false;
        }

        /// <summary>
        /// Determines whether the dictionary contains the specified key.
        /// </summary>
        public bool ContainsKey(TKey key) => TryGetValue(key, out _);

        /// <summary>
        /// Determines whether the dictionary contains the specified value.
        /// </summary>
        public bool ContainsValue(TValue value, IEqualityComparer<TValue>? comparer = null) => Values.Contains(value, comparer);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The value associated with the specified key, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the dictionary contains a value with the specified key, otherwise <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            if (_entryLookup.TryGetValue(key, out var entry)) {
                if (entry.TryGetTarget(out value))
                    return true;
                else
                    _entryLookup.Remove(key);
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Removes internal entries that refer to values that have been garbage collected.
        /// </summary>
        public void Clean()
        {
            foreach (var kvp in _entryLookup.Where(kvp => !kvp.Value.TryGetTarget(out _)))
                _entryLookup.Remove(kvp.Key);

            if (TrimExcessDuringClean)
                TrimExcess();

            _addCountSinceLastClean = 0;
        }

        /// <summary>
        /// Reduces the internal capacity of this dictionary to the size needed to hold the current entries.
        /// </summary>
        public void TrimExcess() => _entryLookup.TrimExcess();

        /// <summary>
        /// Ensures that this dictionary can hold the specified number of elements without growing.
        /// </summary>
        public void EnsureCapacity(int capacity) => _entryLookup.EnsureCapacity(capacity);

        /// <summary>
        /// Returns an enumerator that iterates through the key/value pairs in the dictionary.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in _entryLookup) {
                if (kvp.Value.TryGetTarget(out var value))
                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                else
                    _entryLookup.Remove(kvp.Key);
            }

            _addCountSinceLastClean = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the key/value pairs in the dictionary.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void OnAdded()
        {
            _addCountSinceLastClean++;

            if (_autoCleanAddCount != 0 && _addCountSinceLastClean >= _autoCleanAddCount)
                Clean();
        }
    }
}
