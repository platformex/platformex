using Platformex.Domain;
using System;
using System.Threading.Tasks;

namespace Platformex.Tests
{
    public interface IServiceFixtureExecutor<out TServiceInterface, TService>
        where TService : ServiceBase, TServiceInterface
        where TServiceInterface : IService
    {
        IServiceFixtureAsserter<TServiceInterface, TService> When(Func<TServiceInterface, Task<object>> testFunc);

    }
}