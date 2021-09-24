namespace Platformex.Tests.TestHelpers
{
    public class TestAggregateId : Identity<TestAggregateId>
    {
        public TestAggregateId(string value) : base(value) { }
    }
}