using Orleans;
using System.Threading.Tasks;

namespace Platformex.Domain
{
    public interface ISubscriber<in TIdentity, in TEvent> : IGrainWithStringKey
        where TIdentity : Identity<TIdentity>
        where TEvent : IAggregateEvent<TIdentity>
    {
        Task HandleAsync(IDomainEvent<TIdentity, TEvent> domainEvent);
    }
}