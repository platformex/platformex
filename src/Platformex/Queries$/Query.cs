namespace Platformex
{
    public abstract class Query<TResult> : IQuery<TResult>
    {
        public ICommonMetadata Metadata { get; init; }

        public Query()
        {
            Metadata = new CommandMetadata();
        }

        public Query(ICommandMetadata metadata)
        {
            Metadata = metadata;
        }
    }
}