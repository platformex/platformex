using Orleans;
using System.Threading.Tasks;

namespace Platformex.Application
{
    public interface IReadModel : IGrainWithStringKey
    {
        Task ProcessEvent(IDomainEvent e);
    }
}