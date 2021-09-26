using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IAggregateFixtureArranger<TAggregate, TIdentity, TState>
        where TAggregate : IAggregate<TIdentity>
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        IAggregateFixtureArranger<TAggregate, TIdentity, TState> For(TIdentity aggregateId);
        IAggregateFixtureExecutor<TAggregate, TIdentity, TState> GivenNothing();
        IAggregateFixtureExecutor<TAggregate, TIdentity, TState> GivenUser(string id, string name, string[] roles = null);
        IAggregateFixtureExecutor<TAggregate, TIdentity, TState> Given(params IAggregateEvent<TIdentity>[] aggregateEvents);
        IAggregateFixtureExecutor<TAggregate, TIdentity, TState> Given(TState aggregateState);
        IAggregateFixtureExecutor<TAggregate, TIdentity, TState> Given(params ICommand[] commands);
    }
}