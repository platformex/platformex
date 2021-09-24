
namespace Platformex.Tests.TestHelpers
{
    public record TestedErrorEvent(TestAggregateId Id, int TestErrors) 
        : IAggregateEvent<TestAggregateId>;

    public record TestAddedEvent(TestAggregateId Id, TestEntity Test)
        : IAggregateEvent<TestAggregateId>;

    public record TestAddedEventV2(TestAggregateId Id, string Name, TestEntity Test)
        : IAggregateEvent<TestAggregateId>;

    public record TestCreatedEvent(TestAggregateId Id)
        : IAggregateEvent<TestAggregateId>;

    public record TestCreatedEventV2(TestAggregateId Id, string Name)
        : IAggregateEvent<TestAggregateId>;

    public record TestReceivedEvent(TestAggregateId Id, TestAggregateId SenderAggregateId, TestEntity Test)
        : IAggregateEvent<TestAggregateId>;

    public record TestSentEvent(TestAggregateId Id, TestAggregateId RecipientAggregateId, TestEntity Test)
        : IAggregateEvent<TestAggregateId>;



}
