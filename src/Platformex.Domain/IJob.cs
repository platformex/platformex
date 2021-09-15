using Orleans;
using System.Threading.Tasks;

namespace Platformex.Domain
{
    public interface IJob : IGrainWithStringKey
    {
        Task ExecuteAsync();
    }
}
