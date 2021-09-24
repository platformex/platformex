namespace Platformex
{
    public static class CommandExtensions
    {
        public static TCommand WithMetadata<TCommand>(this TCommand command, CommandMetadata metadata)
            where TCommand : Command
        {
            command.MergeMetadata(metadata);
            return command;
        }

        public static TCommand WithSourceId<TCommand>(this TCommand command, ISourceId commandId)
            where TCommand : Command
        {
            ((CommandMetadata)command.Metadata).SourceId = commandId;
            return command;
        }
    }
}