using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Platformex
{
    public class Metadata : MetadataContainer, IMetadata
    {

        public static IMetadata With(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            return new Metadata(keyValuePairs);
        }

        public static IMetadata With(params KeyValuePair<string, string>[] keyValuePairs)
        {
            return new Metadata(keyValuePairs);
        }

        public static IMetadata With(IDictionary<string, string> keyValuePairs)
        {
            return new Metadata(keyValuePairs);
        }

        [JsonIgnore]
        public ISourceId SourceId
        {
            get => GetMetadataValue(MetadataKeys.SourceId, v => new SourceId(v));
            set => AddOrUpdateValue(MetadataKeys.SourceId, value.Value);
        }

        [JsonIgnore]
        public string EventName
        {
            get => GetMetadataValue(MetadataKeys.EventName);
            set => AddOrUpdateValue(MetadataKeys.EventName, value);
        }

        [JsonIgnore]
        public int EventVersion
        {
            get => GetMetadataValue(MetadataKeys.EventVersion, int.Parse);
            set => AddOrUpdateValue(MetadataKeys.EventVersion, value.ToString());
        }

        [JsonIgnore]
        public DateTimeOffset Timestamp
        {
            get => GetMetadataValue(MetadataKeys.Timestamp, DateTimeOffset.Parse);
            set => AddOrUpdateValue(MetadataKeys.Timestamp, value.ToString("O"));
        }

        [JsonIgnore]
        public long TimestampEpoch => TryGetValue(MetadataKeys.TimestampEpoch, out var timestampEpoch)
            ? long.Parse(timestampEpoch)
            : Timestamp.ToUnixTime();

        [JsonIgnore]
        public int AggregateSequenceNumber
        {
            get => GetMetadataValue(MetadataKeys.AggregateSequenceNumber, int.Parse);
            set => AddOrUpdateValue(MetadataKeys.AggregateSequenceNumber, value.ToString());
        }

        [JsonIgnore]
        public string AggregateId
        {
            get => GetMetadataValue(MetadataKeys.AggregateId);
            set => AddOrUpdateValue(MetadataKeys.AggregateId, value);
        }

        [JsonIgnore]
        public IEventId EventId
        {
            get => GetMetadataValue(MetadataKeys.EventId, Platformex.EventId.With);
            set => AddOrUpdateValue(MetadataKeys.EventId, value.Value);
        }

        [JsonIgnore]
        public string AggregateName
        {
            get => GetMetadataValue(MetadataKeys.AggregateName);
            set => AddOrUpdateValue(MetadataKeys.AggregateName, value);
        }

        public Metadata()
        {
            // Empty
        }

        public Metadata(IDictionary<string, string> keyValuePairs)
            : base(keyValuePairs)
        {
        }

        public Metadata(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
            : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }

        public Metadata(params KeyValuePair<string, string>[] keyValuePairs)
            : this((IEnumerable<KeyValuePair<string, string>>)keyValuePairs)
        {
        }

        public IMetadata CloneWith(params KeyValuePair<string, string>[] keyValuePairs)
        {
            return CloneWith((IEnumerable<KeyValuePair<string, string>>)keyValuePairs);
        }

        public IMetadata CloneWith(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var metadata = new Metadata(this);
            foreach (var kv in keyValuePairs)
            {
                if (metadata.ContainsKey(kv.Key))
                {
                    throw new ArgumentException($"Key '{kv.Key}' is already present!");
                }
                metadata[kv.Key] = kv.Value;
            }
            return metadata;
        }
    }
}