using System.Threading.Tasks;
using Orleans;

namespace Platformex.Application
{

    public interface IQueryHandler : IGrainWithStringKey
    {
        Task<object> QueryAsync(IQuery query);

    }

    public interface IQueryHandler<TResult> : IQueryHandler
    {
        Task<TResult> QueryAsync(IQuery<TResult> query);
    }
}
