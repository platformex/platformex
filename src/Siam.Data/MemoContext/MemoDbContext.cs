using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Siam.Data.MemoContext
{
    public class MemoDbContext : DbContext
    {
        public MemoDbContext(DbContextOptions<MemoDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new MemoModelConfiguration());
        }

        public DbSet<Memo> Memos { get; set; }
    }

    public class MemoDbContextFactory : IDesignTimeDbContextFactory<MemoDbContext>
    {
        public MemoDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MemoDbContext>();
            optionsBuilder.UseSqlServer("Database=Proto;Integrated Security=SSPI;");

            return new MemoDbContext(optionsBuilder.Options);
        }
    }


}