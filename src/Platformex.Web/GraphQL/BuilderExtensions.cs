using System;
using GraphQL;
using GraphQL.Http;
using GraphQL.Server;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Platformex.Infrastructure;

namespace Platformex.Web.GraphQL
{
    public sealed class EventFlyGraphQlOptions
    {
        public EventFlyGraphQlOptions(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; set; }
    }

    public sealed class EventFlyGraphQlConsoleOptions
    {
        public EventFlyGraphQlConsoleOptions(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; set; }
    }

    public static class BuilderExtensions
    {
        public static PlatformBuilder ConfigureGraphQl(this PlatformBuilder builder, Action<EventFlyGraphQlOptions> optionsBuilder)
        {
            var options = new EventFlyGraphQlOptions("graphql");

            builder.AddConfigureServicesActions(services =>
            {
                services.AddSingleton(options);
                services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
                services.AddSingleton<IDocumentWriter, DocumentWriter>();

                services.AddSingleton<Root>();
                services.AddSingleton<ISchema, GraphSchemaInternal>();
                services.AddGraphQL(_ =>
                {
                    _.EnableMetrics = true;
                    _.ExposeExceptions = true;
                });

                foreach (var query in builder.Definitions.Queries)
                {
                    var handlerType = typeof(GraphQueryHandler<,>).MakeGenericType(query.Value.QueryType, query.Value.ResultType);
                    var handlerFullType = typeof(IGraphQueryHandler<,>).MakeGenericType(query.Value.QueryType, query.Value.ResultType);
                    services.AddSingleton(handlerFullType, handlerType);
                    //services.AddSingleton(provider => (IGraphQueryHandler) provider.GetService(handlerFullType));
                }

            });
            return builder;
        }

        public static PlatformBuilder WithConsole(this PlatformBuilder builder, Action<EventFlyGraphQlConsoleOptions> builderOptions)
        {
            var options = new EventFlyGraphQlConsoleOptions("graphql-console");
            builderOptions(options);
            builder.AddConfigureServicesActions(services =>
            {
                services.AddSingleton(options);
                
            });

            return builder;
        }

        //public static IApplicationBuilder UseEventFlyGraphQl(this IApplicationBuilder app)
        //{
        //    var options = app.ApplicationServices.GetRequiredService<EventFlyGraphQlOptions>();
        //    var optionsConsole = app.ApplicationServices.GetRequiredService<EventFlyGraphQlConsoleOptions>();


        //    app.UseGraphQL<ISchema>("/" + options.BasePath.Trim('/'));
        //    app.UseGraphQLPlayground(new GraphQLPlaygroundOptions
        //    {
        //        Path = "/" + optionsConsole.BasePath.Trim('/')
        //    });
        //    return app;
        //}
    }
}
