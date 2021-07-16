using System.Threading.Tasks;
using Demo.Application;
using Demo.Infrastructure.Data;
using Platformex.Application;

namespace Demo.Infrastructure
{

    public class CarDbProvider : IDbProvider<ICarModel>
    {
        private readonly DemoContext _context;

        public CarDbProvider(DemoContext context)
        {
            _context = context;
        }
        public async Task<ICarModel> FindAsync(string id) 
            => await _context.Cars.FindAsync(id);

        public ICarModel Create(string id)
        {
            var model = new CarModel {Id = id};
            _context.Add(model);
            return model;
        }

        public Task SaveChangesAsync(ICarModel _) 
            => _context.SaveChangesAsync();
    }
}
