using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Execution;
using GraphQL.Language.AST;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace Platformex.Web.GraphQL
{
    public sealed class GraphDomain
    {
        private readonly IPlatform _platform;
        private readonly IServiceProvider _provider;
        private readonly Dictionary<String, Type> _handlers = new Dictionary<String, Type>();

        public GraphDomain(IPlatform platform, IServiceProvider provider)
        {
            _platform = platform;
            _provider = provider;
            foreach (var query in _platform.Definitions.Queries)
            {
                var name = query.Value.QueryType.Name;
                name = !name.EndsWith("Query") ? name : name.Substring(0, name.Length - "Query".Length);
                _handlers.Add(name, typeof(IGraphQueryHandler<,>).MakeGenericType(query.Value.QueryType, 
                    query.Value.ResultType));
            }
        }

        public FieldType GetFieldType()
        {
            return new FieldType
            {
                ResolvedType = new ObjectGraphTypeFromDomain("Queries", 
                    _platform.Definitions.Queries.Select(i=>i.Value).ToList(), 
                    _provider),
                Name = GetDomainName("Queries"),
                //Description = _domainDefinition.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description,
                Arguments = new QueryArguments(),
                Resolver = new FuncFieldResolver<Object>(Execute),
            };
        }

        private String GetDomainName(String name)
        {
            return !name.EndsWith("Context") ? name : name.Substring(0, name.Length - "Context".Length);
        }


        private Task<Object> Execute(ResolveFieldContext context)
        {
            foreach (var field in context.FieldAst.SelectionSet.Children.Cast<Field>())
            {
                var queryName = field.Name;

                if (queryName.StartsWith("__")) continue;

                var handler = GetQueryHandler(queryName);
                var f = handler.GetFieldType(true);

                var args = ExecutionHelper.GetArgumentValues(context.Schema, f.Arguments, field.Arguments, new Variables());
                return handler.ExecuteQuery(args);
            }

            return null;
        }


        private IGraphQueryHandler GetQueryHandler(String queryName)
        {
            return (IGraphQueryHandler)_provider.GetService(
                _handlers.FirstOrDefault(i => i.Key.Equals(queryName, StringComparison.InvariantCultureIgnoreCase)).Value);
        }
    }
}