using System;
using System.ComponentModel;
using FluentAssertions;
using Platformex.Tests.TestHelpers;
using Xunit;

namespace Platformex.Tests.UnitTests.Entities
{
    [Category(Categories.Abstractions)]
    [Collection(Collections.Only)]
    public class EntityTests
    {
        [Fact]
        public void InstantiatingEntity_WithNullId_ThrowsException()
        {
            this.Invoking(_ => new TestEntity(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void InstantiatingEntity_WithValidId_HasIdentity()
        {
            var testId = TestId.New;

            var test = new TestEntity(testId);

            test.Id.Should().Be(testId);
        }
    }
}