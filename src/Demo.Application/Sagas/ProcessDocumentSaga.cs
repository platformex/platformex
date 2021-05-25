using System.Threading.Tasks;
using Demo.Cars;
using Demo.Documents;
using Platformex;
using Platformex.Domain;

namespace Demo.Application.Sagas
{
    [EventSubscriber]
    public class ProcessDocumentSaga : Saga<ProcessDocumentSaga>,
        IStartedBy<DocumentId, DocumentCreated>
    
    {
        public async Task<string> HandleAsync(IDomainEvent<DocumentId, DocumentCreated> domainEvent)
        {
            await Domain.CreateCar(CarId.New, domainEvent.AggregateEvent.Name);
            return domainEvent.AggregateIdentity.Value;
        }
    }
}