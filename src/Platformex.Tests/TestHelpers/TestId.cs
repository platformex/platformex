using Newtonsoft.Json;

namespace Platformex.Tests.TestHelpers
{
    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class TestId : Identity<TestId>
    {
        public TestId(string entityId) : base(entityId) { }
    }
}