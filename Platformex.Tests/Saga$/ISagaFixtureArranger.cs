using Platformex.Domain;

namespace Platformex.Tests
{
    public interface ISagaFixtureArranger<TSaga, TSagaState>
        where TSaga : Saga<TSagaState, TSaga> where TSagaState : ISagaState
    {
        ISagaFixtureArranger<TSaga, TSagaState> For();
        ISagaFixtureExecutor<TSaga, TSagaState> GivenNothing();
        ISagaFixtureExecutor<TSaga, TSagaState> Given(params IDomainEvent[] events);
        ISagaFixtureExecutor<TSaga, TSagaState> Given(TSagaState aggregateState);
    }
}