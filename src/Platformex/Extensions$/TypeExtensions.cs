﻿using System;
using System.Linq;

namespace Platformex
{
    public static class TypeExtensions
    {
        public static string GetPrettyName(this IDomainEvent domainEvent)
            => $"{domainEvent?.GetType().Name.Replace("Event","")} => {domainEvent}"; 
        public static string GetPrettyName(this IAggregateEvent aggregateEvent)
            => $"{aggregateEvent?.GetType().Name.Replace("Event","")} => {aggregateEvent}"; 
        public static string GetContextName(Type definition)
            => definition.Namespace?.Split(".").LastOrDefault()?.Replace("Context","");
    }
}