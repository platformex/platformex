using System.Threading.Tasks;

namespace Platformex.Application
{
    public interface IDbProvider<TModel>
    {
        Task<TModel> FindAsync(string id);
        TModel Create(string id);
        Task SaveChangesAsync(TModel model);
    }
}