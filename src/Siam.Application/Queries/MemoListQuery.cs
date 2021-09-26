using Platformex;
using Platformex.Application;
using Siam.MemoContext;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Siam.Application.Queries
{
    public class MemoListQuery : Query<Page<MemoListItem>>
    {
        public int Take { get; set; }
        public int Skip { get; set; }

    }

    public class MemoListItem
    {
        public string Id { get; set; }
        public MemoDocument Document { get; set; }
        public MemoStatus Status { get; set; }
        public ICollection<MemoStatusHistory> History { get; set; } = new List<MemoStatusHistory>();
    }
    public class Page<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
    }

    //Обработчик запроса
    public class DocumentInfoQueryHandler : QueryHandler<MemoListQuery, Page<MemoListItem>>
    {

        protected override Task<Page<MemoListItem>> ExecuteAsync(MemoListQuery query)
        {
            throw new System.NotImplementedException();
        }
    }
}