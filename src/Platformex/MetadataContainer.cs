using System;
using System.Collections.Generic;
using System.Linq;

namespace Platformex
{
    public class MetadataContainer : Dictionary<string, string>
    {
        public MetadataContainer()
        {
        }

        public MetadataContainer(IDictionary<string, string> keyValuePairs)
            : base(keyValuePairs)
        {
        }

        public MetadataContainer(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
            : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }

        public MetadataContainer(params KeyValuePair<string, string>[] keyValuePairs)
            : this((IEnumerable<KeyValuePair<string, string>>)keyValuePairs)
        {
        }

        public void AddRange(params KeyValuePair<string, string>[] keyValuePairs)
        {
            AddRange((IEnumerable<KeyValuePair<string, string>>)keyValuePairs);
        }

        public void AddRange(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        public string GetMetadataValue(string key)
        {
            return GetMetadataValue(key, s => s);
        }

        public void AddOrUpdateValue(string key, string value)
        {
            if (ContainsKey(key)) this[key] = value;
            else Add(key, value);
        }

        public T GetMetadataValue<T>(string key, Func<string, T> converter)
        {
            string value;

            if (!TryGetValue(key, out value))
            {
                throw new MetadataKeyNotFoundException(key);
            }

            try
            {
                return converter(value);
            }
            catch (Exception e)
            {
                throw new MetadataParseException(key, value, e);
            }
        }
        public void Merge(IMetadataContainer metadata)
        {
            foreach (var kv in metadata)
            {
                if (!ContainsKey(kv.Key))
                    Add(kv.Key, kv.Value);
            }
        }
        public string UserId
        {
            get => ContainsKey(MetadataKeys.UserId) ? GetMetadataValue(MetadataKeys.UserId) : null;
            set => AddOrUpdateValue(MetadataKeys.UserId, value);
        }

        public string UserName
        {
            get => ContainsKey(MetadataKeys.UserName) ? GetMetadataValue(MetadataKeys.UserName) : null;
            set => AddOrUpdateValue(MetadataKeys.UserName, value);
        }

#pragma warning disable 659
        public override bool Equals(object obj)
#pragma warning restore 659
        {
            if (obj is MetadataContainer c)
            {
                if (c.Count != Count) return false;
                foreach (var pair in c)
                {
                    if (!ContainsKey(pair.Key)) return false;
                    if (this[pair.Key] != pair.Value) return false;
                }

                return true;
            }

            return false;
        }
    }
}
