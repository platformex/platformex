using FluentAssertions;
using Platformex.Tests.TestHelpers;
using System;
using System.ComponentModel;
using Xunit;

namespace Platformex.Tests.UnitTests.Identity
{
    [Category(Categories.Abstractions)]
    [Collection(Collections.Only)]
    public class IdentityTests : Test
    {
        [Fact]
        public void NewDeterministic_ReturnsKnownResult()
        {
            // Arrange
            var namespaceId = Guid.Parse("769077C6-F84D-46E3-AD2E-828A576AAAF3");
            const string name = "fantastic 4";

            // Arrange
            var testId = TestId.NewDeterministic(namespaceId, name);

            // Assert
            testId.Value.Should().Be("test-da7ab6b1-c513-581f-a1a0-7cdf17109deb");
            TestId.IsValid(testId.Value).Should().BeTrue();
        }

        [Theory]
        [InlineData("test-da7ab6b1-c513-581f-a1a0-7cdf17109deb", "da7ab6b1-c513-581f-a1a0-7cdf17109deb")]
        [InlineData("test-00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        public void WithValidValue(string value, string expectedGuidValue)
        {
            // Arrange
            TestId testId = null;
            var expectedGuid = Guid.Parse(expectedGuidValue);

            // Act
            var exception = Record.Exception(() => testId = TestId.With(value));
            Assert.Null(exception);

            // Assert
            testId.Should().NotBeNull();
            testId.Value.Should().Be(value);
            testId.GetGuid().Should().Be(expectedGuid);
        }

        [Fact]
        public void InputOutput()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var testId = TestId.With(guid);

            // Assert
            testId.GetGuid().Should().Be(guid);
        }

        [Fact]
        public void ShouldBeLowerCase()
        {
            // Act
            var testId = TestId.New;

            // Assert
            testId.Value.Should().Be(testId.Value.ToLowerInvariant());
        }

        [Fact]
        public void New_IsValid()
        {
            // Arrange
            var testId = TestId.New;

            // Assert
            TestId.IsValid(testId.Value).Should().BeTrue(testId.Value);
        }

        [Fact]
        public void NewComb_IsValid()
        {
            // Arrange
            var testId = TestId.NewComb();

            // Assert
            TestId.IsValid(testId.Value).Should().BeTrue(testId.Value);
        }

        [Fact]
        public void NewDeterministic_IsValid()
        {
            // Arrange
            var testId = TestId.NewDeterministic(Guid.NewGuid(), Guid.NewGuid().ToString());

            // Assert
            TestId.IsValid(testId.Value).Should().BeTrue(testId.Value);
        }

        [Theory]
        [InlineData("da7ab6b1-c513-581f-a1a0-7cdf17109deb")]
        [InlineData("TestId-da7ab6b1-c513-581f-a1a0-7cdf17109deb")]
        [InlineData("test-769077C6-F84D-46E3-AD2E-828A576AAAF3")]
        [InlineData("test-pppppppp-pppp-pppp-pppp-pppppppppppp")]
        [InlineData("funny-da7ab6b1-c513-581f-a1a0-7cdf17109deb")]
        [InlineData(null)]
        [InlineData("")]
        public void CannotCreateBadIds(string badIdValue)
        {
            // Act
            Assert.Throws<ArgumentException>(() => TestId.With(badIdValue)).Message.Should().Contain("Identity is invalid:");
        }

        public class Id : Identity<Id>
        {
            public Id(string value) : base(value)
            {
            }
        }

        [Fact]
        public void JustId()
        {
            // Arrange
            var guid = A<Guid>();
            var expected = guid.ToString("D");

            // Act
            var id = Id.With(guid);

            // Assert
            id.Value.Should().Be(expected);
        }
    }
}
