using System;
using System.ComponentModel;
using FluentAssertions;
using Xunit;

namespace Platformex.Tests.UnitTests.Identity
{
    [Category(Categories.Abstractions)]
    [Collection(Collections.Only)]
    public class SourceIdTests
    {
        [Fact]
        public void InstantiatingSourceId_WithNullString_ThrowsException()
        {
            this.Invoking(_ => new SourceId(null))
                .Should().Throw<ArgumentNullException>();
        }
    }
}