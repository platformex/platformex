using System.Collections.Generic;
using System.Threading.Tasks;
using Platformex.Application;
using Platformex.Domain;

namespace Platformex.Tests.TestHelpers
{
    public interface ITestAggregateState : IAggregateState<TestAggregateId>,
    ICanApply<TestAddedEvent, TestAggregateId>,
    ICanApply<TestReceivedEvent, TestAggregateId>,
    ICanApply<TestSentEvent, TestAggregateId>,
    ICanApply<TestCreatedEvent, TestAggregateId>
    {
        public List<TestEntity> TestCollection { get; }
    }
    public class TestAggregateState : AggregateState<TestAggregateId, TestAggregateState>, ITestAggregateState
    {
        public List<TestEntity> TestCollection { get; private set; }

        public void Apply(TestCreatedEvent aggregateEvent)
        {
            TestCollection = new List<TestEntity>();
        }

        public void Apply(TestAddedEvent aggregateEvent)
        {
            TestCollection.Add(aggregateEvent.Test);
        }

        public void Apply(TestReceivedEvent aggregateEvent)
        {
            TestCollection.Add(aggregateEvent.Test);
        }

        public void Apply(TestSentEvent aggregateEvent)
        {
            TestCollection.RemoveAll(x => x.Id == aggregateEvent.Test.Id);
        }

        protected override Task<bool> LoadStateInternal(TestAggregateId id) => Task.FromResult(true);

        public override Task BeginTransaction() => Task.CompletedTask;

        public override Task CommitTransaction() => Task.CompletedTask;


        public override Task<bool> RollbackTransaction() => Task.FromResult(true);

    }
}