using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Platformex
{
    public class QueryMetadata : Metadata, ICommonMetadata
    {
        public QueryMetadata()
        {
        }
        public QueryMetadata(ISourceId sourceId)
        {
            SourceId = sourceId ?? Platformex.SourceId.New;
        }

        public QueryMetadata(IDictionary<string, string> keyValuePairs)
            : base(keyValuePairs)
        {
        }

        public QueryMetadata(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
            : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }

        public QueryMetadata(params KeyValuePair<string, string>[] keyValuePairs)
            : this((IEnumerable<KeyValuePair<string, string>>)keyValuePairs)
        {
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
    }
}