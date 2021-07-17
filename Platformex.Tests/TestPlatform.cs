using System;
using System.Threading.Tasks;

namespace Platformex.Tests
{
    public class TestPlatform : IPlatform
    {
        public Definitions Definitions { get; } = new Definitions();
        
        public event EventHandler<EventPublishedArgs> EventPublished;

        public Task<CommandResult> ExecuteAsync(string aggregateId, ICommand command)
        {
            throw new NotImplementedException();
        }

        public Task PublishEvent(IDomainEvent domainEvent)
        {
            
            if (EventPublished != null) 
                EventPublished(this, new EventPublishedArgs(domainEvent));
            
            return Task.CompletedTask;
        }

        public TAggregate GetAggregate<TAggregate>(string id) where TAggregate : IAggregate
        {
            throw new NotImplementedException();
        }

        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
        {
            throw new NotImplementedException();
        }

        public Task<object> QueryAsync(IQuery query)
        {
            throw new NotImplementedException();
        }
    }

    public class EventPublishedArgs : EventArgs
    {
        public IDomainEvent DomainEvent { get; }

        public EventPublishedArgs(IDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}