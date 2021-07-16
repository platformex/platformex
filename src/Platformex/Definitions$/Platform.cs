using System.Threading.Tasks;

namespace Platformex
{
    public interface IDomain
    {
        TAggregate GetAggregate<TAggregate>(string id)
            where TAggregate : IAggregate;

    }
    public interface IPlatform : IDomain
    {
        Definitions Definitions { get; }
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query);
        Task<object> QueryAsync(IQuery query);
        Task<CommandResult> ExecuteAsync(string aggragateId, ICommand command);
    }
}