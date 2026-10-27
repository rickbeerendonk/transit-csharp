// Copyright © 2014 Rick Beerendonk. All Rights Reserved.
//
// This code is a C# port of the Java version created and maintained by Cognitect, therefore
//
// Copyright © 2014 Cognitect. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS-IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Beerendonk.Transit.Impl;

internal sealed class TransitDictionary : IDictionary<object, object>, IDictionary
{
    private static readonly object NullKey = new object();
    private readonly Dictionary<object, object> values = new Dictionary<object, object>();

    private static object NormalizeKey(object key)
    {
        return key ?? NullKey;
    }

    private static object DenormalizeKey(object key)
    {
        return ReferenceEquals(key, NullKey) ? null : key;
    }

    public object this[object key]
    {
        get => values[NormalizeKey(key)];
        set => values[NormalizeKey(key)] = value;
    }

    public ICollection<object> Keys => values.Keys.Select(DenormalizeKey).ToArray();

    public ICollection<object> Values => values.Values;

    public int Count => values.Count;

    public bool IsReadOnly => false;

    public void Add(object key, object value)
    {
        values.Add(NormalizeKey(key), value);
    }

    public bool ContainsKey(object key)
    {
        return values.ContainsKey(NormalizeKey(key));
    }

    public bool Remove(object key)
    {
        return values.Remove(NormalizeKey(key));
    }

    public bool TryGetValue(object key, out object value)
    {
        return values.TryGetValue(NormalizeKey(key), out value);
    }

    public void Add(KeyValuePair<object, object> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        values.Clear();
    }

    public bool Contains(KeyValuePair<object, object> item)
    {
        object value;
        return TryGetValue(item.Key, out value)
            && EqualityComparer<object>.Default.Equals(value, item.Value);
    }

    public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
    {
        foreach (KeyValuePair<object, object> item in this)
        {
            array[arrayIndex++] = item;
        }
    }

    public bool Remove(KeyValuePair<object, object> item)
    {
        return Contains(item) && Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
    {
        foreach (KeyValuePair<object, object> item in values)
        {
            yield return new KeyValuePair<object, object>(
                DenormalizeKey(item.Key),
                item.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IDictionary)this).GetEnumerator();
    }

    object IDictionary.this[object key]
    {
        get
        {
            object value;
            return TryGetValue(key, out value) ? value : null;
        }
        set => this[key] = value;
    }

    ICollection IDictionary.Keys => new ArrayList(Keys.ToArray());

    ICollection IDictionary.Values => new ArrayList(Values.ToArray());

    bool IDictionary.IsReadOnly => false;

    bool IDictionary.IsFixedSize => false;

    void IDictionary.Add(object key, object value)
    {
        Add(key, value);
    }

    bool IDictionary.Contains(object key)
    {
        return ContainsKey(key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return new Enumerator(GetEnumerator());
    }

    void IDictionary.Remove(object key)
    {
        Remove(key);
    }

    void ICollection.CopyTo(Array array, int index)
    {
        foreach (KeyValuePair<object, object> item in this)
        {
            array.SetValue(new DictionaryEntry(item.Key, item.Value), index++);
        }
    }

    int ICollection.Count => Count;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    private sealed class Enumerator(IEnumerator<KeyValuePair<object, object>> values) : IDictionaryEnumerator
    {
        public DictionaryEntry Entry => new DictionaryEntry(Key, Value);

        public object Key => values.Current.Key;

        public object Value => values.Current.Value;

        public object Current => Entry;

        public bool MoveNext()
        {
            return values.MoveNext();
        }

        public void Reset()
        {
            values.Reset();
        }
    }
}
