using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IAggregateFixtureExecutor<TAggregate, TIdentity, TState>
        where TAggregate : IAggregate<TIdentity>
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        IAggregateFixtureAsserter<TAggregate, TIdentity, TState> When(params ICommand[] commands);
    }
}