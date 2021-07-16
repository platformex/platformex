namespace Platformex
{
    public interface IQuery
    {

    }
    // ReSharper disable once UnusedTypeParameter
    public interface IQuery<TResult> : IQuery
    {
        ICommonMetadata Metadata { get; init; }

    }
}
