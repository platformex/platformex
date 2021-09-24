using System;
using System.ComponentModel;
using FluentAssertions;
using Platformex.Tests.TestHelpers;
using Xunit;

namespace Platformex.Tests.UnitTests.Abstractions.Commands
{
    [Category(Categories.Abstractions)]
    [Collection(Collections.Only)]
    public class CommandTests
    {
        [Fact]
        public void InstantiatingCommand_WithValidInput_ThrowsException()
        {
            var aggregateId = TestAggregateId.New;
            var sourceId = SourceId.New;

            var command = new CreateTestCommand(aggregateId)
                .WithMetadata(new CommandMetadata(sourceId));

            command.Metadata.SourceId.Value.Should().Be(sourceId.Value);
        }

        [Fact]
        public void InstantiatingCommand_WithNullId_ThrowsException()
        {
            this.Invoking(_ => new CreateTestCommand(null))
                .Should().Throw<ArgumentNullException>().And.Message.Contains("aggregateId").Should().BeTrue();
        }
    }
}