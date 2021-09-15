using Platformex;
using Platformex.Domain;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Siam.MemoContext.Domain
{
    [Subscriber]
    public class MemoSigningSubscriber : Subscriber<MemoId, MemoUpdated>
    {
        public override async Task HandleAsync(IDomainEvent<MemoId, MemoUpdated> domainEvent)
        {
            var result = await ExecuteAsync(new SignMemo(domainEvent.AggregateEvent.Id, SecurityContext.UserId));
            if (!result.IsSuccess)
                Debug.WriteLine(result.Error);
        }
    }
}