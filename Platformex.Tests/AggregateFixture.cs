using System;
using System.Collections.Generic;
using Platformex.Application;
using Platformex.Domain;
using Xunit;

namespace Platformex.Tests
{
    public class AggregateFixture<TIdentity, TAggregate, TState, TStateImpl> : IFixtureArranger<TAggregate, TIdentity,TState>,
        IFixtureExecutor<TAggregate, TIdentity, TState>, IFixtureAsserter<TAggregate, TIdentity,TState>
        where TAggregate : Aggregate<TIdentity, TState, TAggregate>
        where TIdentity : Identity<TIdentity>
        where TState :  IAggregateState<TIdentity>
        where TStateImpl : AggregateState<TIdentity,TStateImpl>, TState
    {
        private readonly PlatformexTestKit _testKit;
        private TAggregate _aggregate;
        private TState State => _aggregate.TestOnlyGetState(); 
        public TIdentity AggregateId => _aggregate?.GetId<TIdentity>();
        private readonly Stack<CommandResult> _commandResults = new Stack<CommandResult>();
        private readonly Stack<IDomainEvent> _events = new Stack<IDomainEvent>();
        
        private bool _isMonitoring;
        private void StopMonitoring() => _isMonitoring = false;
        private void StartMonitoring()=> _isMonitoring = true;


        public AggregateFixture(PlatformexTestKit testKit)
        {
            _testKit = testKit;
            _testKit.Builder.RegisterAggregate<TIdentity, TAggregate, TStateImpl>().WithCommands();
        }
        public IFixtureArranger<TAggregate, TIdentity, TState> For(TIdentity aggregateId)
        {
            _testKit.Platform.EventPublished += (sender, args) =>
            {
                if (_isMonitoring) 
                    _events.Push(args.DomainEvent);
            };
            
            _aggregate = _testKit.TestKitSilo.CreateGrainAsync<TAggregate>(aggregateId.Value).GetAwaiter().GetResult();
            return this;
        }

        public IFixtureExecutor<TAggregate, TIdentity, TState> GivenNothing() => this;
        public IFixtureExecutor<TAggregate, TIdentity, TState> Given(params IAggregateEvent<TIdentity>[] aggregateEvents)
        {
            foreach (var aggregateEvent in aggregateEvents)
            {
                State.Apply(aggregateEvent);
            }
            return this;
        }


        public IFixtureExecutor<TAggregate, TIdentity, TState> Given(TState aggregateState)
        {
            _aggregate.TestOnlySetState(aggregateState);
            return this;
        }

        public IFixtureExecutor<TAggregate, TIdentity, TState> Given(params ICommand[] commands)
        {
            foreach (var command in commands)
            {
                _aggregate.DoAsync(command).GetAwaiter().GetResult();
            }

            return this;
        }


        public IFixtureAsserter<TAggregate, TIdentity, TState> When(params ICommand[] commands)
        {
            StartMonitoring();
            foreach (var command in commands)
            {
                _commandResults.Push(_aggregate.DoAsync(command).GetAwaiter().GetResult());
            }
            StopMonitoring();
            return this;
        }


        public IFixtureAsserter<TAggregate, TIdentity, TState> AndWhen(params ICommand[] commands)
        {
            foreach (var command in commands)
            {
                _aggregate.DoAsync(command).GetAwaiter().GetResult();
            }

            return this;
        }

        public IFixtureAsserter<TAggregate, TIdentity, TState> ThenExpect<TAggregateEvent>(
            Predicate<TAggregateEvent> aggregateEventPredicate = null) 
            where TAggregateEvent : class, IAggregateEvent<TIdentity>
        {
            Assert.True(_events.Pop().GetType() == typeof(TAggregateEvent));
            return this;
        }

        public IFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectState(Predicate<TState> aggregateEventPredicate = null)
        {
            Assert.True(aggregateEventPredicate?.Invoke(State));
            return this;
        }

        public IFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectResult(Predicate<CommandResult> aggregateReply = null)
        {
            Assert.True(aggregateReply?.Invoke(_commandResults.Pop()));
            return this;
        }


        public IFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectDomainEvent<TAggregateEvent>(
            Predicate<IDomainEvent<TIdentity, TAggregateEvent>> domainEventPredicate = null) 
            where TAggregateEvent : class, IAggregateEvent<TIdentity>
        {
            Assert.True(domainEventPredicate?.Invoke((IDomainEvent<TIdentity, TAggregateEvent>)_events.Pop()));
            return this;
        }
    }
}