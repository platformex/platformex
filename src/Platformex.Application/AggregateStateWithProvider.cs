using System.Threading.Tasks;

namespace Platformex.Application
{
    public interface IModel
    {
        string Id { get; set; }
    }
    public abstract class AggregateStateWithProvider<TIdentity, TEventApplier, TModel> :AggregateState<TIdentity, TEventApplier> 
        where TIdentity : Identity<TIdentity>
        where TModel : IModel
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly IDbProvider<TModel> Provider;
        protected TModel Model;
        protected AggregateStateWithProvider(IDbProvider<TModel> provider)
        {
            Provider = provider;
        }
        protected override async Task LoadStateInternal(TIdentity id)
        {
            Model = await Provider.FindAsync(id.Value) 
                     ?? Provider.Create(id.Value);
            Model.Id ??= id.Value;
        }

        public override Task BeginTransaction() => Provider.BeginTransaction();
        public override Task CommitTransaction() => Provider.CommitTransaction();
        public override async Task RollbackTransaction()
        {
            await Provider.RollbackTransaction();
            await LoadStateInternal(Id);
        }

        protected override async Task AfterApply(IAggregateEvent<TIdentity> @event)
        {
            await Provider.SaveChangesAsync(Model.Id, Model);
        }

    }
}