using System;
using System.Collections;
using System.Collections.Generic;

namespace Singulink.Collections
{
    /// <summary>
    /// Represents a collection of weakly referenced values. If this collection is accessed from multiple threads, all accesses must be synchronized with a
    /// lock.
    /// </summary>
    /// <remarks>
    /// <para>Internal entries for garbage collected values are not removed automatically by default. You can perform a full clean by calling the <see
    /// cref="Clean"/> method or configure automatic cleaning after a set number of <see cref="Add(T)"/> operations by setting the <see
    /// cref="AutoCleanAddCount"/> property.</para>
    /// </remarks>
    public sealed class WeakCollection<T> : IEnumerable<T> where T : class
    {
        private readonly List<WeakReference<T>> _list = new List<WeakReference<T>>();

        private int _autoCleanAddCount;
        private int _extraTrimExcessCapacity;
        private int _addCountSinceLastClean;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakCollection{T}"/> class.
        /// </summary>
        public WeakCollection()
        {
        }

        /// <summary>
        /// Gets or sets the number of <see cref="Add(T)"/> operations that automatically trigger the <see cref="Clean"/> method to run. Default value is
        /// <c>0</c> which indicates that automatic cleaning is not performed.
        /// </summary>
        public int AutoCleanAddCount {
            get => _autoCleanAddCount;
            set {
                if (_autoCleanAddCount < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _autoCleanAddCount = value;
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
        /// Gets or sets the extra capacity to leave when <see cref="TrimExcess"/> is called. Default value is <c>0</c>.
        /// </summary>
        public int ExtraTrimExcessCapacity {
            get => _extraTrimExcessCapacity;
            set {
                if (_extraTrimExcessCapacity < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _extraTrimExcessCapacity = value;
            }
        }

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
        public int UnreliableCount => _list.Count;

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        public void Add(T item)
        {
            _addCountSinceLastClean++;

            if (_autoCleanAddCount != 0 && _addCountSinceLastClean >= _autoCleanAddCount)
                Clean();

            _list.Add(new WeakReference<T>(item));
        }

        /// <summary>
        /// Removes an item from the collection using the default equality comparer.
        /// </summary>
        /// <returns><see langword="true"/> if the item was removed, otherwise <see langword="false"/>.</returns>
        public bool Remove(T item) => Remove(item, null);

        /// <summary>
        /// Removes an item from the collection using the specified equality comparer.
        /// </summary>
        /// <returns><see langword="true"/> if the item was removed, otherwise <see langword="false"/>.</returns>
        public bool Remove(T item, IEqualityComparer<T>? comparer)
        {
            comparer ??= EqualityComparer<T>.Default;

            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].TryGetTarget(out var value)) {
                    if (comparer.Equals(value, item)) {
                        _list.RemoveAt(i);
                        return true;
                    }
                }
                else {
                    _list.RemoveAt(i);
                    i--;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the collection contains the given item.
        /// </summary>
        public bool Contains(T item) => Contains(item, null);

        /// <summary>
        /// Determines whether the collection contains the given item using the specified equality comparer.
        /// </summary>
        public bool Contains(T item, IEqualityComparer<T>? comparer)
        {
            comparer ??= EqualityComparer<T>.Default;

            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].TryGetTarget(out var value) && comparer.Equals(value, item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes all the elements from the collection.
        /// </summary>
        public void Clear() => _list.Clear();

        /// <summary>
        /// Removes internal entries for values that have been garbage collected and trims the excess if <see cref="TrimExcessDuringClean"/> is set.
        /// </summary>
        public void Clean()
        {
            _list.RemoveAll(entry => !entry.TryGetTarget(out _));

            if (TrimExcessDuringClean)
                TrimExcess();

            _addCountSinceLastClean = 0;
        }

        /// <summary>
        /// Reduces the internal capacity to the number of entries in the collection plus <see cref="ExtraTrimExcessCapacity"/>.
        /// </summary>
        public void TrimExcess()
        {
            int trimmedCapacity = _list.Count + ExtraTrimExcessCapacity;

            if (trimmedCapacity < _list.Capacity)
                _list.Capacity = trimmedCapacity;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var entry in _list) {
                if (entry.TryGetTarget(out var item))
                    yield return item;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
