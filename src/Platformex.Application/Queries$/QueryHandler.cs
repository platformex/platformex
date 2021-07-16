using System;
using System.Threading.Tasks;
using Orleans;
using Platformex.Domain;

namespace Platformex.Application
{
    public abstract class QueryHandler<TQuery, TResult> : Grain, IQueryHandler<TResult>
    where TQuery : IQuery<TResult>
    {
        protected SecurityContext SecurityContext { get; private set; }
        public Task<TResult> QueryAsync(IQuery<TResult> query)
        {
            var sc = new SecurityContext(query.Metadata);
            //Проверим права
            var requiredUser = SecurityContext.IsUserRequiredFrom(query);
            if (requiredUser && !sc.IsAuthorized)
                throw new UnauthorizedAccessException();
            
            var requiredRole = SecurityContext.GetRolesFrom(query);
            if (requiredRole != null)
                sc.HasRoles(requiredRole);

            SecurityContext = sc;
            return ExecuteAsync((TQuery) query);
        }

        protected abstract Task<TResult> ExecuteAsync(TQuery query);
        public async Task<object> QueryAsync(IQuery query) => await QueryAsync((IQuery<TResult>) query);
    }
}
