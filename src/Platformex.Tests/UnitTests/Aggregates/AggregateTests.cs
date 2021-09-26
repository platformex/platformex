using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentAssertions;
using FluentValidation.Results;
using Orleans.TestKit;
using Platformex.Tests.TestHelpers;
using Xunit;

namespace Platformex.Tests.UnitTests.Aggregates
{
    [Category(Categories.Domain)]
    [Collection(Collections.Only)]
    public class AggregateTests : PlatformexTestKit
    {
        public AggregateTests()
        {
            Silo.AddService<ITestAggregateState>(new TestAggregateState());
        }
        [Fact]
        public void InitialState_AfterAggregateCreation_TestCreatedEventEmitted()
        {

            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new CreateTestCommand(aggregateId).WithSourceId(commandId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                    ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectDomainEvent<TestCreatedEvent>(
                x => x.AggregateEvent.Id.Equals(aggregateId) &&
                x.Metadata.ContainsKey("some-key"));
        }

        [Fact]
        public void SendingCommand_ToAggregateRoot_ShouldReplyWithProperMessage()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new CreateTestCommand(aggregateId).WithSourceId(commandId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectResult(result => result.IsSuccess);
        }

        [Fact]
        public void EventContainerMetadata_AfterAggregateCreation_TestCreatedEventEmitted()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new CreateTestCommand(aggregateId).WithSourceId(commandId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectDomainEvent<TestCreatedEvent>(
                x => x.AggregateEvent.Id.Equals(aggregateId)
                && x.IdentityType == typeof(TestAggregateId)
                && x.EventType == typeof(TestCreatedEvent)
                && x.Metadata.EventName == "TestCreated"
                && x.Metadata.EventVersion == 1
                && x.Metadata.SourceId.Value == commandId.Value);
        }

        [Fact]
        public void InitialState_AfterAggregateCreation_TestStateSignalled()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new CreateTestCommand(aggregateId).WithSourceId(commandId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectDomainEvent<TestCreatedEvent>()
                .ThenExpectState(
                x => x.TestCollection.Count == 0);
        }

        [Fact]
        public void TestCommand_AfterAggregateCreation_TestEventEmitted()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new CreateTestCommand(aggregateId).WithSourceId(commandId);
            var testId = TestId.New;
            var test = new TestEntity(testId);
            var nextCommandId = SourceId.New;
            var nextCommand = new AddTestCommand(aggregateId, test).WithSourceId(nextCommandId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command, nextCommand)
                .ThenExpectDomainEvent<TestCreatedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>(
                    x => x.AggregateEvent.Test.Equals(test));
        }

        [Fact]
        public void TestCommandTwice_AfterAggregateCreation_TestEventEmitted()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new CreateTestCommand(aggregateId).WithSourceId(commandId);
            var testId = TestId.New;
            var test = new TestEntity(testId);
            var nextCommandId = SourceId.New;
            var nextCommand = new AddTestCommand(aggregateId, test).WithSourceId(nextCommandId);
            var test2Id = TestId.New;
            var test2 = new TestEntity(test2Id);
            var nextCommandId2 = SourceId.New;
            var nextCommand2 = new AddTestCommand(aggregateId, test2).WithSourceId(nextCommandId2);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command, nextCommand, nextCommand2)
                .ThenExpectDomainEvent<TestCreatedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>(
                    x => x.AggregateEvent.Test.Equals(test))
                .ThenExpectDomainEvent<TestAddedEvent>(
                    x => x.AggregateEvent.Test.Equals(test2));
        }

        [Fact]
        public void TestEventSourcing_AfterManyTests_TestStateSignalled()
        {
            IEnumerable<ICommand> GetCommands(TestAggregateId testAggregateId, ISourceId commandId1)
            {
                var command = new CreateTestCommand(testAggregateId).WithSourceId(commandId1);
                yield return command;

                for (var i = 0; i < 5; i++)
                {
                    var test = new TestEntity(TestId.New);
                    var testCommandId = SourceId.New;
                    var testCommand = new AddTestCommand(testAggregateId, test).WithSourceId(testCommandId);
                    yield return testCommand;
                }

                var poisonCommand = new PoisonTestAggregateCommand(testAggregateId);
                yield return poisonCommand;

            }

            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            
            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(GetCommands(aggregateId, commandId).ToArray())
                .ThenExpectDomainEvent<TestCreatedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectState(
                    x=>x.TestCollection.Count == 5);
        }

        [Fact]
        public void TestEventMultipleEmitSourcing_AfterManyMultiCreateCommand_EventsEmitted()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var firstTest = new TestEntity(TestId.New);
            var secondTest = new TestEntity(TestId.New);
            var command = new CreateAndAddTwoTestsCommand(aggregateId, firstTest, secondTest)
                .WithSourceId(commandId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectDomainEvent<TestCreatedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>();
        }

        [Fact]
        public void TestEventMultipleEmitSourcing_AfterManyMultiCommand_TestStateSignalled()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;

            var command = new CreateTestCommand(aggregateId).WithSourceId(commandId);

            var test = new TestEntity(TestId.New);
            var testSourceId = SourceId.New;
            var testCommand = new AddFourTestsCommand(aggregateId, test).WithSourceId(testSourceId);

            var poisonCommand = new PoisonTestAggregateCommand(aggregateId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command,testCommand,poisonCommand)
                .ThenExpectDomainEvent<TestCreatedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectDomainEvent<TestAddedEvent>()
                .ThenExpectState(
                    x => x.TestCollection.Count == 4);
        }

        [Fact]
        public void InitialState_TestingSuccessCommand_SuccessResultReplied()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new TestSuccessExecutionResultCommand(aggregateId).WithSourceId(commandId);
            
            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectResult(r => r.IsSuccess);

        }

        [Fact]
        public void InitialState_TestingFailedCommand_SuccessResultReplied()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new TestFailedExecutionResultCommand(aggregateId).WithSourceId(commandId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectResult(r => !r.IsSuccess);
        }

        [Fact]
        public void InitialState_TestingValidatedCommand_SuccessResultReplied()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new ValidatedCommand(aggregateId, true).WithSourceId(commandId);

            var result = RulesHelper.ProcessRules(command);
            result.Should().NotBeNull().And.Match<ValidationResult>(r => r.IsValid);

        }

        [Fact]
        public void InitialState_TestingBadCommand_SuccessResultReplied()
        {
            var aggregateId = TestAggregateId.New;
            var commandId = SourceId.New;
            var command = new BadCommand(aggregateId).WithSourceId(commandId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectResult(r => !r.IsSuccess);
        }

        [Fact]
        public void InitialState_TestingUnauthorized()
        {
            var aggregateId = TestAggregateId.New;
            var command = new TestUnauthorizedCommand(aggregateId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectResult(r => r.Should().BeOfType<UnauthorizedResult>().And.Ok());
        }

        [Fact]
        public void InitialState_TestingUnauthorized2()
        {
            var aggregateId = TestAggregateId.New;
            var command = new TestUnauthorizedCommand2(aggregateId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectResult(r => r.Should().BeOfType<UnauthorizedResult>().And.Ok());
        }
        [Fact]
        public void InitialState_TestingUnauthorized3()
        {
            var aggregateId = TestAggregateId.New;
            var command = new TestUnauthorizedCommand(aggregateId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenUser("test", "Test User")
                .When(command)
                .ThenExpectResult(r => r.Should().BeEquivalentTo(Result.Success).And.Ok());
        }

        [Fact]
        public void InitialState_TestingForbidden()
        {
            var aggregateId = TestAggregateId.New;
            var command = new TestForbiddenCommand(aggregateId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectResult(r => r.Should().BeOfType<ForbiddenResult>().And.Ok());
        }

        [Fact]
        public void InitialState_TestingForbidden2()
        {
            var aggregateId = TestAggregateId.New;
            var command = new TestForbiddenCommand2(aggregateId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenNothing()
                .When(command)
                .ThenExpectResult(r => r.Should().BeOfType<ForbiddenResult>().And.Ok());
        }
        [Fact]
        public void InitialState_TestingForbidden3()
        {
            var aggregateId = TestAggregateId.New;
            var command = new TestForbiddenCommand(aggregateId);

            var fixture = new AggregateFixture<TestAggregateId, TestAggregate, 
                ITestAggregateState, TestAggregateState>(this);

            fixture.For(aggregateId)
                .GivenUser("test", "Test User", new []{"Admin"})
                .When(command)
                .ThenExpectResult(r => r.Should().BeEquivalentTo(Result.Success).And.Ok());
        }

    }
}