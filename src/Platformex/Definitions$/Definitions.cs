using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#region hack

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endregion

namespace Platformex
{



    public record AggregateDefinition(Type IdentityType, Type AggregateType, Type InterfaceType, Type StateType);
    public record CommandDefinition(string Context, string Name, Type IdentityType, Type CommandType, bool IsPublic);
    public record QueryDefinition(string Name, Type QueryType, Type ResultType, bool IsPublic);
    public record ServiceDefinition(string Context, string Name, Type ServiceType, Type InterfaceType, string MethodName,
        ParameterInfo[] Parameters, Type ReturnType, bool IsPublic);


    public sealed class Definitions
    {
        public readonly Dictionary<Type, AggregateDefinition> Aggregates = new();

        public readonly Dictionary<string, CommandDefinition> Commands = new();
        public readonly Dictionary<string, QueryDefinition> Queries = new();
        public readonly Dictionary<string, ServiceDefinition> Services = new();
        public AggregateDefinition Aggregate<TIdentity>() where TIdentity : Identity<TIdentity>
            => Aggregates[typeof(TIdentity)];
        private readonly List<Assembly> _applicationPartsAssemblies = new();

        public void Register(AggregateDefinition definition)
        {
            Aggregates.Add(definition.IdentityType, definition);
        }
        public void Register(CommandDefinition definition)
        {
            var key = (definition.Context + ":" + definition.Name).ToLower();
            Commands.Add(key, definition);
        }
        public void Register(QueryDefinition definition)
        {
            Queries.Add(definition.Name, definition);
        }
        public void Register(ServiceDefinition definition)
        {
            var key = (definition.Context + ":" + definition.MethodName).ToLower();
            Services.Add(key, definition);
        }
        public IEnumerable<Assembly> Assemblies =>
            Aggregates.Values.SelectMany(i => new[]
                {
                    i.AggregateType.Assembly,
                    i.IdentityType.Assembly,
                    i.InterfaceType.Assembly,
                    i.StateType.Assembly
                }).Concat(_applicationPartsAssemblies)
                .Distinct();

        public void RegisterApplicationParts(Assembly contextAppliactionParts)
        {
            if (_applicationPartsAssemblies.Contains(contextAppliactionParts)) return;

            _applicationPartsAssemblies.Add(contextAppliactionParts);

        }

        public bool TryGetDefinition(string context, string name, out CommandDefinition commandDefinition)
        {
            var key = (context + ":" + name).ToLower();
            if (Commands.ContainsKey(key))
            {
                commandDefinition = Commands[key];
                return true;
            }

            commandDefinition = null;
            return false;
        }

        public bool TryGetDefinition(string context, string name, out ServiceDefinition serviceDefinition)
        {
            var key = (context + ":" + name).ToLower();
            if (Services.ContainsKey(key))
            {
                serviceDefinition = Services[key];
                return true;
            }

            serviceDefinition = null;
            return false;
        }

        public bool TryGetDefinition(string name, out QueryDefinition queryDefinition)
        {
            if (Queries.ContainsKey(name))
            {
                queryDefinition = Queries[name];
                return true;
            }

            queryDefinition = null;
            return false;
        }

    }
}