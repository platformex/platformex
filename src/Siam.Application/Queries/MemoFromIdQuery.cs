using Platformex;
using Siam.MemoContext;
using System.Collections.Generic;

namespace Siam.Application.Queries
{
    public class MemoFromIdQuery : Query<MemoListItem>
    {
        public string MemoId { get; set; }
    }

    public class MemoItem
    {
        public string Id { get; set; }
        public MemoDocument Document { get; set; }
        public IEnumerable<MemoStatusHistory> History { get; set; }
    }

}