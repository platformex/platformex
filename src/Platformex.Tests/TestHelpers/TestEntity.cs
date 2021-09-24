namespace Platformex.Tests.TestHelpers
{
    public class TestEntity : Entity<TestId>
    {
        public static TestEntity New => new TestEntity(TestId.New);

        public TestEntity(TestId id) : base(id) { }
    }
}