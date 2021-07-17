using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IFixtureArranger<TAggregate, TIdentity,TState>
        where TAggregate : IAggregate<TIdentity>
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        IFixtureArranger<TAggregate, TIdentity, TState> For(TIdentity aggregateId);
        IFixtureExecutor<TAggregate, TIdentity, TState> GivenNothing();
        IFixtureExecutor<TAggregate, TIdentity, TState> Given(params IAggregateEvent<TIdentity>[] aggregateEvents);
        IFixtureExecutor<TAggregate, TIdentity, TState> Given(TState aggregateState);
        IFixtureExecutor<TAggregate, TIdentity, TState> Given(params ICommand[] commands);
    }
}