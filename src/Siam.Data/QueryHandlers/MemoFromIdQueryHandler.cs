using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Platformex.Application;
using Siam.Application.Queries;
using Siam.Data.MemoContext;

namespace Siam.Data.QueryHandlers
{
    //Обработчик запроса
    public class MemoFromIdQueryHandler : QueryHandler<MemoFromIdQuery, MemoListItem>
    {
        private readonly MemoDbContext _context;

        public MemoFromIdQueryHandler(MemoDbContext context)
        {
            _context = context;
        }

        protected override Task<MemoListItem> ExecuteAsync(MemoFromIdQuery query)
        {
            return _context.Memos.Where(i => i.Id == query.MemoId)
                .Select(i => new MemoListItem
                {
                    Id = i.Id,
                    Document = i.Model.Document,
                    History = i.Model.History,
                    Status = i.Model.Status
                }).FirstOrDefaultAsync();
        }
    }
}