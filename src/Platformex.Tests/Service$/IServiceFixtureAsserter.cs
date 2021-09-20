﻿using Platformex.Domain;
using System;
using System.Threading.Tasks;

namespace Platformex.Tests
{
    public interface IServiceFixtureAsserter<out TServiceInterface, TService>
        where TService : ServiceBase, TServiceInterface
        where TServiceInterface : IService
    {
        IServiceFixtureAsserter<TServiceInterface, TService> AndWhen(Func<TServiceInterface, Task<object>> testFunc);

        IServiceFixtureAsserter<TServiceInterface, TService> ThenExpect<TIdentity, TCommand>(Predicate<TCommand> commandPredicate = null)
            where TCommand : ICommand<TIdentity> where TIdentity : Identity<TIdentity>;

        IServiceFixtureAsserter<TServiceInterface, TService> ThenExpectResult<TResult>(Predicate<TResult> resultPredicate = null);
    }
}