using System;
using System.Linq;
using System.Threading.Tasks;
using Platformex.Domain;

namespace Platformex.Tests.TestHelpers
{
    public interface ITestAggregate : IAggregate<TestAggregateId>,
    ICanDo<CreateTestCommand, TestAggregateId>,
    ICanDo<CreateAndAddTwoTestsCommand, TestAggregateId>,
    ICanDo<AddTestCommand, TestAggregateId>,
    ICanDo<AddFourTestsCommand, TestAggregateId>,
    ICanDo<GiveTestCommand, TestAggregateId>,
    ICanDo<ReceiveTestCommand, TestAggregateId>,
    ICanDo<PoisonTestAggregateCommand, TestAggregateId>,
    ICanDo<TestDomainErrorCommand, TestAggregateId>,
    ICanDo<TestFailedExecutionResultCommand, TestAggregateId>,
    ICanDo<TestSuccessExecutionResultCommand, TestAggregateId>,
    ICanDo<BadCommand, TestAggregateId>
    {

    }
    public sealed class TestAggregate : Aggregate<TestAggregateId, ITestAggregateState, TestAggregate>, ITestAggregate
    {
        public int TestErrors { get; private set; }

        public TestAggregate()
        {

            TestErrors = 0;
        }

        public async Task<Result> Do(CreateTestCommand command)
        {
            if (IsNew)
            {
                await Emit(new TestCreatedEvent(command.Id), new EventMetadata { { "some-key", "some-value" } });
                return Result.Success;
            }

            TestErrors++;
            await Emit(new TestedErrorEvent(Id, TestErrors));
            return Result.Fail("TestedErrorEvent");
        }

        public async Task<Result> Do(CreateAndAddTwoTestsCommand command)
        {
            if (IsNew)
            {
                var createdEvent = new TestCreatedEvent(command.Id);
                var firstTestAddedEvent = new TestAddedEvent(command.Id, command.FirstTest);
                var secondTestAddedEvent = new TestAddedEvent(command.Id, command.SecondTest);
                await Emit(createdEvent);
                await Emit(firstTestAddedEvent);
                await Emit(secondTestAddedEvent);
                return Result.Success;
            }

            TestErrors++;
            await Emit(new TestedErrorEvent(Id, TestErrors));

            return Result.Fail("TestedErrorEvent");
        }

        public async Task<Result> Do(AddTestCommand command)
        {
            if (!IsNew)
            {
                await Emit(new TestAddedEvent(Id, command.Test));
                return Result.Success;
            }

            TestErrors++;
            await Emit(new TestedErrorEvent(Id, TestErrors));
            return Result.Fail("TestedErrorEvent");
        }

        public async Task<Result> Do(AddFourTestsCommand command)
        {
            if (!IsNew)
            {
                foreach (var _ in Enumerable.Range(0, 4))
                {
                    await Emit(new TestAddedEvent(Id, command.Test));
                }

                return Result.Success;
            }

            TestErrors++;
            await Emit(new TestedErrorEvent(Id, TestErrors));
            return Result.Fail("TestedErrorEvent");
        }

        public async Task<Result> Do(GiveTestCommand command)
        {
            if (!IsNew)
            {
                if (State.TestCollection.Any(x => x.Id == command.TestToGive.Id))
                {
                    await Emit(new TestSentEvent(Id, command.ReceiverAggregateId, command.TestToGive));
                    return Result.Success;
                }
            }

            TestErrors++;
            await Emit(new TestedErrorEvent(Id, TestErrors));
            return Result.Fail("TestedErrorEvent");
        }

        public async Task<Result> Do(ReceiveTestCommand command)
        {
            if (!IsNew)
            {
                await Emit(new TestReceivedEvent(Id, command.SenderAggregateId, command.TestToReceive));
                return Result.Success;
            }

            TestErrors++;
            await Emit(new TestedErrorEvent(Id, TestErrors));
            return Result.Fail("TestedErrorEvent");
        }

        public Task<Result> Do(TestFailedExecutionResultCommand command)
        {
            return Task.FromResult(Result.Fail("ERROR"));
        }

        public Task<Result> Do(TestSuccessExecutionResultCommand command)
        {
            return Task.FromResult(Result.Success);
        }

        public async Task<Result> Do(PoisonTestAggregateCommand command)
        {
            if (!IsNew)
            {
                DeactivateOnIdle();
            }
            else
            {
                TestErrors++;
                await Emit(new TestedErrorEvent(Id, TestErrors));
                return Result.Fail("TestedErrorEvent");
            }

            return Result.Success;
        }

        public async Task<Result> Do(TestDomainErrorCommand command)
        {
            TestErrors++;
            await Emit(new TestedErrorEvent(Id, TestErrors));

            return Result.Success;
        }

        public Task<Result> Do(BadCommand command)
        {
            throw new Exception("Bad Command");
        }
    }
}