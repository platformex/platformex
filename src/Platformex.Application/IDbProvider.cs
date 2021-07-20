using System.Threading.Tasks;

namespace Platformex.Application
{
    public interface IDbProvider<TModel>
    {
        Task<(TModel model, bool isCreated)> LoadOrCreate(string id);
        Task SaveChangesAsync(string id,  TModel model);
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
    }
}