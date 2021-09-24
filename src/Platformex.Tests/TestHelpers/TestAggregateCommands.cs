
using FluentValidation;

namespace Platformex.Tests.TestHelpers
{
    public record CreateTestCommand(TestAggregateId Id)
        : Command, ICommand<TestAggregateId>; 

    public record AddFourTestsCommand(TestAggregateId Id, TestEntity Test) 
        : Command, ICommand<TestAggregateId>;

    public record AddTestCommand(TestAggregateId Id, TestEntity Test)
        : Command, ICommand<TestAggregateId>;

    public record BadCommand(TestAggregateId Id)
        : Command, ICommand<TestAggregateId>;

    public record CreateAndAddTwoTestsCommand(TestAggregateId Id, TestEntity FirstTest, TestEntity SecondTest)
        : Command, ICommand<TestAggregateId>;

    public record GiveTestCommand(TestAggregateId Id, TestAggregateId ReceiverAggregateId, TestEntity TestToGive)
        : Command, ICommand<TestAggregateId>;

    public record PoisonTestAggregateCommand(TestAggregateId Id)
        : Command, ICommand<TestAggregateId>;

    public record ReceiveTestCommand(TestAggregateId Id, TestAggregateId SenderAggregateId, TestEntity TestToReceive)
        : Command, ICommand<TestAggregateId>;

    public record  TestDomainErrorCommand(TestAggregateId Id)
        : Command, ICommand<TestAggregateId>;

    public record TestFailedExecutionResultCommand(TestAggregateId Id)
        : Command, ICommand<TestAggregateId>;

    public record TestSuccessExecutionResultCommand(TestAggregateId Id)
        : Command, ICommand<TestAggregateId>;

    [Rules(typeof(ValidatedCommandRules))]
    public record ValidatedCommand(TestAggregateId Id, bool IsValid)
        : Command, ICommand<TestAggregateId>;

    public sealed class ValidatedCommandRules : Rules<ValidatedCommand>
    {
        public ValidatedCommandRules()
        {
            RuleFor(x => x.IsValid).Must(x => x);
        }
    }

}
