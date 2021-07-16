namespace Platformex
{
    public interface ICommandId : ISourceId
    {
    }
    public class CommandId : Identity<CommandId>, ICommandId
    {
        public CommandId(string value)
            : base(value)
        {
        }
    }
}