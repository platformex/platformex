using Platformex;

namespace Siam.Application.Queries
{
    public class MemoListQuery : IQuery<Page<MemoListItem>>
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        
    }

    public class MemoListItem
    {
        public string Id { get; set; }
    }
}