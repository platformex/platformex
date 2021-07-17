using Orleans.TestKit;
using Platformex.Application;
using Platformex.Tests;
using Siam.Application;
using Siam.MemoContext;
using Siam.MemoContext.Domain;
using Xunit;

namespace Siam.Tests
{
    public class UnitTest1 : PlatformexTestKit
    {
        public UnitTest1()
        {
            Silo.AddService<IMemoState>(new MemoState(new InMemoryDbProvider<MemoModel>()));
        }

        [Fact]
        public void Test1()
        {
            var id = MemoId.New;
            var fixture = new AggregateFixture<MemoId, MemoAggregate, IMemoState, MemoState>(this);

            fixture
                .For(id)
                .GivenNothing()
                .When(new RejectMemo(id, string.Empty, RejectionReason.Undefined))
                .ThenExpectResult(e => !e.IsSuccess)
                //.ThenExpectDomainEvent<RejectionStarted>(e 
                //    => e.AggregateEvent.Id == id && e.AggregateEvent.RejectionReason == RejectionReason.Undefined)
                .ThenExpectState(s => s.Status == MemoStatus.Undefined);
        }
    }
}