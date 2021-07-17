using System;
using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IFixtureAsserter<TAggregate, TIdentity, TState>
        where TAggregate : IAggregate<TIdentity>
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        IFixtureAsserter<TAggregate, TIdentity, TState> AndWhen(params ICommand[] commands);

        IFixtureAsserter<TAggregate, TIdentity, TState> ThenExpect<TAggregateEvent>(Predicate<TAggregateEvent> aggregateEventPredicate = null)
            where TAggregateEvent : class, IAggregateEvent<TIdentity>;

        IFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectState(
            Predicate<TState> aggregateEventPredicate = null);
            

        IFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectResult(Predicate<CommandResult> aggregateReply = null);

        IFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectDomainEvent<TAggregateEvent>(Predicate<IDomainEvent<TIdentity, TAggregateEvent>> domainEventPredicate = null)
            where TAggregateEvent : class, IAggregateEvent<TIdentity>;
    }
}