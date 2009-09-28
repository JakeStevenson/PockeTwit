using System;

using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace PockeTwit.LocalStorage
{
    /// <summary>
    /// Generic cache class with settable size. Items requested recently will be the last to be destroyed.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class CacheDictionary<TKey, TValue> where TValue : IDisposable
    {
        private object _lock = new object();

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        /// <param name="maxItemCount"></param>
        public CacheDictionary(int maxItemCount, int purgeCount)
        {
            MaxItemCount = maxItemCount;
            PurgeCount = Math.Min(maxItemCount, purgeCount);
        }

        private int _maxItemCount;
        /// <summary>
        /// Gets or sets the maximum item count.
        /// </summary>
        public int MaxItemCount
        {
            get
            {
                return _maxItemCount;
            }
            set
            {
                _maxItemCount = value;
                PurgeItems();
            }
        }

        /// <summary>
        /// Gets or sets the number of items that will be purged if the count exceeds the maximum item count.
        /// </summary>
        public int PurgeCount { get; set; }

        private Dictionary<TKey, TValue> _cache = new Dictionary<TKey,TValue>();
        /// <summary>
        /// Deletes the oldest items ntil the total object count is below MaxItemCount
        /// </summary>
        private void PurgeItems()
        {
            lock (_lock)
            {
                if (_cache.Count <= MaxItemCount)
                    return;

                while (_cache.Count > MaxItemCount-PurgeCount)
                {
                    Remove((TKey)_keyList[0]);
                }
            }
        }

        /// <summary>
        /// Removes a key from the cache.
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            lock (_lock)
            {
                (_cache[key] as IDisposable).Dispose();
                _cache.Remove(key);
                _keyList.Remove(key);
            }
        }

        /// <summary>
        /// Marks a key as touched, changing the order of purges and makes sure the requested item will be the last to be destroyed.
        /// </summary>
        /// <param name="key"></param>
        private void TouchKey(object key)
        {
            lock (_lock)
            {
                _keyList.Remove(key);
                _keyList.Add(key);
            }
        }

        private ArrayList _keyList = new ArrayList();
        /// <summary>
        /// Adds an item to the cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                _cache.Add(key, value);
                _keyList.Add(key);
                PurgeItems();
            }
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                foreach (TKey key in _cache.Keys)
                {
                    (_cache[key] as IDisposable).Dispose();
                }
                _cache.Clear();
                _keyList.Clear();
            }
        }

        /// <summary>
        /// Retrieves an item from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result = false;
            lock (_lock)
            {
                result = _cache.TryGetValue(key, out value);
                if (result)
                {
                    // promote to end of list
                    TouchKey(key);
                    return result;
                }
            }
            return result;
        }
    }
}
