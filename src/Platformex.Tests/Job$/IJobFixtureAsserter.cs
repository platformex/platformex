using Platformex.Domain;
using System;

namespace Platformex.Tests
{
    public interface IJobFixtureAsserter<TJob>
        where TJob : Job
    {
        IJobFixtureAsserter<TJob> ThenExpect<TCommandIdentity, TCommand>(
            Predicate<TCommand> commandPredicate = null)
            where TCommandIdentity : Identity<TCommandIdentity> where TCommand : ICommand<TCommandIdentity>;
    }
}