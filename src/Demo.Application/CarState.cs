using System.Threading.Tasks;
using Demo.Cars;
using Demo.Cars.Domain;
using Platformex;
using Platformex.Application;

namespace Demo.Application
{
    public interface ICarModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class CarState : AggregateState<CarId, CarState>, ICarState,
        ICanApply<CarCreated, CarId>,
        ICanApply<CarRenamed, CarId>
    {
        private readonly IDbProvider<ICarModel> _provider;
        private ICarModel _model;

        public CarState(IDbProvider<ICarModel> provider)
        {
            _provider = provider;
        }

        public string Name => _model.Name;

        public void Apply(CarCreated e) 
            => _model.Name = e.Name;
        public void Apply(CarRenamed e)
            => _model.Name = e.NewName;

        protected override async Task LoadStateInternal(CarId id)
        {
            _model = await _provider.FindAsync(id.Value) ?? _provider.Create(id.Value);
        }

        protected override async Task AfterApply(IAggregateEvent<CarId> id)
        {
            await _provider.SaveChangesAsync(_model);
        }
    }
}