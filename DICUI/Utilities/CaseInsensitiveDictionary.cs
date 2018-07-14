using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DICUI.Utilities
{
    public class CaseInsensitiveDictionary<TValue> : IDictionary<string, TValue>
    {
        private Dictionary<string, TValue> _dict = new Dictionary<string, TValue>();

        public TValue this[string key]
        {
            get
            {
                key = key.ToLower();
                if (_dict.ContainsKey(key))
                    return _dict[key];
                throw new ArgumentException("Key could not be found in the dictionary");
            }
            set
            {
                key = key.ToLower();
                _dict[key] = value;
            }
        }

        public ICollection<string> Keys => _dict.Keys;

        public ICollection<TValue> Values => _dict.Values;

        public int Count => _dict.Count;

        public bool IsReadOnly => false;

        public void Add(string key, TValue value)
        {
            key = key.ToLower();
            _dict[key] = value;
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            string key = item.Key.ToLower();
            _dict[key] = item.Value; ;
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            KeyValuePair<string, TValue> temp = new KeyValuePair<string, TValue>(item.Key.ToLower(), item.Value);
            return _dict.Contains(temp);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key.ToLower());
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _dict.Remove(key.ToLower());
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out TValue value)
        {
            key = key.ToLower();
            return _dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }
}
