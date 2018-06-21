using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DICUI.External
{
	// Adapted from https://www.codeproject.com/Articles/18615/OrderedDictionary-T-A-generic-implementation-of-IO
	public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
	{
		private List<KeyValuePair<TKey, TValue>> _list;
		private Dictionary<TKey, TValue> _dictionary;
		private int _count;

		int ICollection.Count => _count;
		int ICollection<KeyValuePair<TKey, TValue>>.Count => _count;

		ICollection IDictionary.Keys => _dictionary.Keys;
		ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dictionary.Keys;

		ICollection IDictionary.Values => _dictionary.Values;
		ICollection<TValue> IDictionary<TKey, TValue>.Values => _dictionary.Values;

		bool IDictionary.IsReadOnly => false;
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		bool IDictionary.IsFixedSize => false;

		object ICollection.SyncRoot => new object();

		bool ICollection.IsSynchronized => true;

		public TValue this[int index]
		{
			get
			{
				return _list[index].Value;
			}
			set
			{
				if (index >= _count || index < 0)
					throw new ArgumentOutOfRangeException("index",
						  "'index' must be non-negative and less than" +
						  " the size of the collection");

				TKey key = _list[index].Key;

				_list[index] = new KeyValuePair<TKey, TValue>(key, value);
				_dictionary[key] = value;
			}
		}

		object IOrderedDictionary.this[int index]
		{
			get
			{
				return _list[index].Value;
			}
			set
			{
				if (index >= _count || index < 0)
					throw new ArgumentOutOfRangeException("index",
						  "'index' must be non-negative and less than" +
						  " the size of the collection");

				var valueObj = (TValue)value;
				if (valueObj == null)
					throw new ArgumentException($"Value must be of type {typeof(TValue)}");

				TKey key = _list[index].Key;

				_list[index] = new KeyValuePair<TKey, TValue>(key, valueObj);
				_dictionary[key] = valueObj;
			}
		}

		object IDictionary.this[object key]
		{
			get
			{
				var keyObj = (TKey)key;
				if (keyObj == null)
					throw new ArgumentException($"Key must be of type {typeof(TKey)}");

				return _dictionary[keyObj];
			}
			set
			{
				var keyObj = (TKey)key;
				if (keyObj == null)
					throw new ArgumentException($"Key must be of type {typeof(TKey)}");

				var valueObj = (TValue)value;
				if (valueObj == null)
					throw new ArgumentException($"Value must be of type {typeof(TValue)}");

				if (_dictionary.ContainsKey(keyObj))
				{
					_dictionary[keyObj] = valueObj;
					_list[IndexOfKey(keyObj)] = new KeyValuePair<TKey, TValue>(keyObj, valueObj);
				}
				else
				{
					Add(keyObj, valueObj);
				}
			}
		}

		TValue IDictionary<TKey, TValue>.this[TKey key]
		{
			get
			{
				return _dictionary[key];
			}
			set
			{
				if (_dictionary.ContainsKey(key))
				{
					_dictionary[key] = value;
					_list[IndexOfKey(key)] = new KeyValuePair<TKey, TValue>(key, value);
				}
				else
				{
					Add(key, value);
				}
			}
		}

		public OrderedDictionary()
		{
			_list = new List<KeyValuePair<TKey, TValue>>();
			_dictionary = new Dictionary<TKey, TValue>();
			_count = 0;
		}

		public int Add(TKey key, TValue value)
		{
			_dictionary.Add(key, value);
			_list.Add(new KeyValuePair<TKey, TValue>(key, value));
			return _count - 1;
		}

		public void Insert(int index, TKey key, TValue value)
		{
			if (index > _count || index < 0)
				throw new ArgumentOutOfRangeException("index");

			_dictionary.Add(key, value);
			_list.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
		}

		void IOrderedDictionary.RemoveAt(int index)
		{
			if (index >= _count || index < 0)
				throw new ArgumentOutOfRangeException("index",
					  "'index' must be non-negative and less than " +
					  "the size of the collection");

			TKey key = _list[index].Key;

			_list.RemoveAt(index);
			_dictionary.Remove(key);
		}

		public bool Remove(TKey key)
		{
			if (null == key)
				throw new ArgumentNullException("key");

			int index = IndexOfKey(key);
			if (index >= 0)
			{
				if (_dictionary.Remove(key))
				{
					_list.RemoveAt(index);
					return true;
				}
			}
			return false;
		}

		private int IndexOfKey(TKey key)
		{
			return _list.FindIndex(kvp => kvp.Key.Equals(key));
		}

		IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		void IOrderedDictionary.Insert(int index, object key, object value)
		{
			var keyObj = (TKey)key;
			if (keyObj == null)
				throw new ArgumentException($"Key must be of type {typeof(TKey)}");

			var valueObj = (TValue)value;
			if (valueObj == null)
				throw new ArgumentException($"Value must be of type {typeof(TValue)}");

			Insert(index, keyObj, valueObj);
		}

		bool IDictionary.Contains(object key)
		{
			var keyObj = (TKey)key;
			if (keyObj == null)
				throw new ArgumentException($"Key must be of type {typeof(TKey)}");

			return _dictionary.ContainsKey(keyObj);
		}

		void IDictionary.Add(object key, object value)
		{
			var keyObj = (TKey)key;
			if (keyObj == null)
				throw new ArgumentException($"Key must be of type {typeof(TKey)}");

			var valueObj = (TValue)value;
			if (valueObj == null)
				throw new ArgumentException($"Value must be of type {typeof(TValue)}");

			Add(keyObj, valueObj);
		}

		void IDictionary.Clear()
		{
			_dictionary.Clear();
			_list.Clear();
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		void IDictionary.Remove(object key)
		{
			var keyObj = (TKey)key;
			if (keyObj == null)
				throw new ArgumentException($"Key must be of type {typeof(TKey)}");

			Remove(keyObj);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			var arrayObj = array as KeyValuePair<TKey, TValue>[];
			if (arrayObj == null)
				throw new ArgumentException($"Key must be of type {typeof(KeyValuePair<TKey, TValue>[])}");

			_list.CopyTo(arrayObj, index);
		}

		bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			Add(key, value);
		}

		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			return Remove(key);
		}

		bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Clear()
		{
			_dictionary.Clear();
			_list.Clear();
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			return _list.Contains(item);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _list.GetEnumerator();
		}
	}
}
