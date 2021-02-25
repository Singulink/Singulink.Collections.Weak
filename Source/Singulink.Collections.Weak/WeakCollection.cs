using System;
using System.Collections;
using System.Collections.Generic;

namespace Singulink.Collections
{
    /// <summary>
    /// Represents a collection of weakly referenced values that keeps items in an undefined order. If this collection is accessed from multiple threads, all
    /// accesses must be synchronized with a lock.
    /// </summary>
    /// <remarks>
    /// <para>Internal entries for garbage collected values are removed as they are encountered, i.e. as they are enumerated over. You can perform a full clean
    /// by calling the <see cref="Clean"/> method or configure automatic cleaning after a set number of add operations by setting the <see
    /// cref="AutoCleanAddCount"/> property.</para>
    /// </remarks>
    public sealed class WeakCollection<T> : IEnumerable<T> where T : class
    {
        private readonly HashSet<WeakReference<T>> _entries = new();

        private int _autoCleanAddCount;
        private int _addCountSinceLastClean;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakCollection{T}"/> class.
        /// </summary>
        public WeakCollection()
        {
        }

        /// <summary>
        /// Gets or sets the number of <see cref="Add(T)"/> operations that automatically triggers the <see cref="Clean"/> method to run. Default value is
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
        /// Gets the number of add operations that have been performed since the last cleaning.
        /// </summary>
        public int AddCountSinceLastClean => _addCountSinceLastClean;

        /// <summary>
        /// Gets or sets a value indicating whether to automatically call <see cref="TrimExcess"/> whenever <see cref="Clean"/> is called. Default value is
        /// <see langword="false"/>.
        /// </summary>
        public bool TrimExcessDuringClean { get; set; }

        /// <summary>
        /// Gets the approximate number of values in this collection. This value may be considerably off if there are lots of garbage collected values that
        /// still have internal entries in the collection.
        /// </summary>
        /// <remarks>
        /// <para>This count may not be accurate if values have been collected since the last clean or enumeration. You can call <see cref="Clean"/> to force a
        /// full sweep before reading the count to get a more accurate value. If you require an accurate count then you should copy the values into a new
        /// strongly referenced collection (i.e. a list) so that they can't be garbage collected and use that collection to obtain the count and access the
        /// values.</para>
        /// </remarks>
        public int UnreliableCount => _entries.Count;

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        public void Add(T item)
        {
            _entries.Add(new WeakReference<T>(item));
            _addCountSinceLastClean++;

            if (_autoCleanAddCount != 0 && _addCountSinceLastClean >= _autoCleanAddCount)
                Clean();
        }

        /// <summary>
        /// Removes an item from the collection using the specified equality comparer.
        /// </summary>
        /// <returns><see langword="true"/> if the item was removed, otherwise <see langword="false"/>.</returns>
        public bool Remove(T item, IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;

            foreach (var entry in _entries) {
                if (entry.TryGetTarget(out var value)) {
                    if (comparer.Equals(value, item)) {
                        _entries.Remove(entry);
                        return true;
                    }
                }
                else {
                    _entries.Remove(entry);
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the collection contains the given item using the specified equality comparer.
        /// </summary>
        public bool Contains(T item, IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;

            foreach (var entry in _entries) {
                if (entry.TryGetTarget(out var value)) {
                    if (comparer.Equals(value, item))
                        return true;
                }
                else {
                    _entries.Remove(entry);
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all the elements from the collection.
        /// </summary>
        public void Clear() => _entries.Clear();

        /// <summary>
        /// Removes internal entries for values that have been garbage collected and trims the excess if <see cref="TrimExcessDuringClean"/> is set.
        /// </summary>
        public void Clean()
        {
            foreach (var entry in _entries) {
                if (!entry.TryGetTarget(out var item))
                    _entries.Remove(entry);
            }

            if (TrimExcessDuringClean)
                TrimExcess();

            _addCountSinceLastClean = 0;
        }

        /// <summary>
        /// Reduces the internal capacity to the number of entries in the collection.
        /// </summary>
        public void TrimExcess() => _entries.TrimExcess();

        /// <summary>
        /// Ensures that this collection can hold the specified number of elements without growing.
        /// </summary>
        public void EnsureCapacity(int capacity) => _entries.EnsureCapacity(capacity);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var entry in _entries) {
                if (entry.TryGetTarget(out var item))
                    yield return item;
                else
                    _entries.Remove(entry);
            }

            _addCountSinceLastClean = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
