using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IFixtureExecutor<TAggregate, TIdentity, TState>
        where TAggregate : IAggregate<TIdentity>
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        IFixtureAsserter<TAggregate, TIdentity, TState> When(params ICommand[] commands);
    }
}