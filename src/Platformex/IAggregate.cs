﻿using Orleans;
using System.Threading.Tasks;

namespace Platformex
{
    public interface IAggregate : IGrainWithStringKey
    {
        Task<Result> DoAsync(ICommand command);
    }

    public interface IAggregate<out T> : IAggregate where T : Identity<T>
    {
    }
}