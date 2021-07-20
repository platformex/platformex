﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Platformex.Tests
{
    public class TestPlatform : IPlatform
    {
        public Definitions Definitions { get; } = new Definitions();
        
        public event EventHandler<EventPublishedArgs> EventPublished;
        public event EventHandler<CommandExecutedArgs> CommandExecuted;

        public Task<CommandResult> ExecuteAsync(string aggregateId, ICommand command)
        {
            if (CommandExecuted != null) 
                CommandExecuted(this, new CommandExecutedArgs(command));
            
            return Task.FromResult(_results.TryPop(out var result) ? result : CommandResult.Success);
        }

        public Task PublishEvent(IDomainEvent domainEvent)
        {
            
            if (EventPublished != null) 
                EventPublished(this, new EventPublishedArgs(domainEvent));
            
            return Task.CompletedTask;
        }

        public TAggregate GetAggregate<TAggregate>(string id) where TAggregate : IAggregate
        {
            throw new NotImplementedException();
        }

        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
        {
            throw new NotImplementedException();
        }

        public Task<object> QueryAsync(IQuery query)
        {
            throw new NotImplementedException();
        }

        private readonly Stack<CommandResult> _results = new Stack<CommandResult>();
        public void SetCommandResults(CommandResult[] results)
        {
            foreach (var result in results) _results.Push(result);
        }

        public void ClearCommandResults()
        {
            _results.Clear();
        }
    }

    public class EventPublishedArgs : EventArgs
    {
        public IDomainEvent DomainEvent { get; }

        public EventPublishedArgs(IDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
    public class CommandExecutedArgs : EventArgs
    {
        public ICommand Command { get; }

        public CommandExecutedArgs(ICommand command)
        {
            Command = command;
        }
    }
}