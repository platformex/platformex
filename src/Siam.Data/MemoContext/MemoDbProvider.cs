using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Platformex.Application;
using Siam.Application;

namespace Siam.Data.MemoContext
{

    public class MemoDbProvider : IDbProvider<MemoModel>
    {
        private readonly MemoDbContext _dbContext;

        public MemoDbProvider(MemoDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<MemoModel> FindAsync(string id) 
            => await _dbContext.Memos.Where(i => i.Id == id).Select(i=>i.Model).FirstOrDefaultAsync();

        public MemoModel Create(string id) => new MemoModel {Id = id};

        public async Task SaveChangesAsync(MemoModel model)
        {
            var item = new Memo { Id = model.Id, Model = model};
            if (await _dbContext.Memos.AnyAsync(i => i.Id == model.Id))
                _dbContext.Update(item);
            else
                _dbContext.Add(item);
            await _dbContext.SaveChangesAsync();
            
            _dbContext.Entry(item).State = EntityState.Detached;
        }
    }
}
