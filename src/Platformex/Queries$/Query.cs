namespace Platformex
{

    public abstract class Query
    {
        public QueryMetadata Metadata { get; init; }

        internal void MergeMetadata(QueryMetadata metadata)
        {
            Metadata.Merge(metadata);
        }

    }

    public abstract class Query<TResult> : Query, IQuery<TResult>
    {

        protected Query()
        {
            Metadata = new QueryMetadata();
        }

        protected Query(QueryMetadata metadata)
        {
            Metadata = metadata;
        }
    }
}