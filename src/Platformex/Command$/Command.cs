namespace Platformex
{
    public interface ICommand
    {
        ICommandMetadata Metadata { get; init; }
    }

    public interface ICommand<T> : ICommand where T : Identity<T>
    {
        T Id { get; }
    }

    public abstract record Command : ICommand
    {
        public ICommandMetadata Metadata { get; init; }

        public Command()
        {
            Metadata = new CommandMetadata(SourceId.New);
        }
        
        public Command(ICommandMetadata metadata)
        {
            Metadata = metadata;
        }

    }
    
    
    
}