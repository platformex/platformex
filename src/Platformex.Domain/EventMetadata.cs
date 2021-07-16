using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Platformex.Domain
{
    public class EventMetadata : MetadataContainer, IEventMetadata
    {
        public static IEventMetadata Empty { get; } = new EventMetadata();

        public static IEventMetadata With(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            return new EventMetadata(keyValuePairs);
        }

        public static IEventMetadata With(params KeyValuePair<string, string>[] keyValuePairs)
        {
            return new EventMetadata(keyValuePairs);
        }

        public static IEventMetadata With(IDictionary<string, string> keyValuePairs)
        {
            return new EventMetadata(keyValuePairs);
        }

        public ISourceId SourceId
        {
            get { return GetMetadataValue(MetadataKeys.SourceId, v => new SourceId(v)); }
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
        public long TimestampEpoch
        {
            get
            {
#pragma warning disable IDE0018 // Inline variable declaration
                string timestampEpoch;
#pragma warning restore IDE0018 // Inline variable declaration
                return TryGetValue(MetadataKeys.TimestampEpoch, out timestampEpoch)
                    ? long.Parse(timestampEpoch)
                    : Timestamp.ToUnixTime();
            }
        }

        [JsonIgnore]
        public long AggregateSequenceNumber
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
        public string CorrelationId
        {
            get => GetMetadataValue(MetadataKeys.CorrelationId);
            set => AddOrUpdateValue(MetadataKeys.CorrelationId, value);
        }

        [JsonIgnore]
        public IReadOnlyCollection<string> CorrelationIds
        {
            get => ContainsKey(MetadataKeys.CorrelationIds) ?
                GetMetadataValue(MetadataKeys.CorrelationIds)?.Split(',') :
                new List<string>().AsReadOnly();
            set => AddOrUpdateValue(MetadataKeys.CorrelationIds, value == null ? "" : string.Join(",", value));
        }


        [JsonIgnore]
        public string CausationId
        {
            get => GetMetadataValue(MetadataKeys.CausationId);
            set => AddOrUpdateValue(MetadataKeys.CausationId, value);
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

        public EventMetadata()
        {
            // Empty
        }

        public EventMetadata(IDictionary<string, string> keyValuePairs)
            : base(keyValuePairs)
        {
        }

        public EventMetadata(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
            : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }

        public EventMetadata(params KeyValuePair<string, string>[] keyValuePairs)
            : this((IEnumerable<KeyValuePair<string, string>>)keyValuePairs)
        {
        }

        public IEventMetadata CloneWith(params KeyValuePair<string, string>[] keyValuePairs)
        {
            return CloneWith((IEnumerable<KeyValuePair<string, string>>)keyValuePairs);
        }

        public IEventMetadata CloneWith(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var metadata = new EventMetadata(this);
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
