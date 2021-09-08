using System;
using GraphQL.Server;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Platformex.Infrastructure;

namespace Platformex.Web.GraphQL
{
    public sealed class PlatformexGraphQlOptions
    {
        public PlatformexGraphQlOptions(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; set; }
    }

    public sealed class PlatformexGraphQlConsoleOptions
    {
        public PlatformexGraphQlConsoleOptions(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; set; }
    }

    public static class BuilderExtensions
    {

        public static PlatformBuilder ConfigureGraphQl(this PlatformBuilder builder, Action<PlatformexGraphQlOptions> optionsBuilder)
        {

            var options = new PlatformexGraphQlOptions("graphql");

            builder.AddConfigureServicesActions(services =>
            {
                optionsBuilder?.Invoke(options);
                services.AddSingleton(options);
                //services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
                //services.AddSingleton<IDocumentWriter, DocumentWriter>();

                services.AddSingleton<Root>();
                services.AddSingleton<ISchema, GraphSchemaInternal>();
                services.AddGraphQL(_ =>
                    {
                        _.EnableMetrics = true;
                        //_.ExposeExceptions = true;
                    })
                    .AddSystemTextJson()
                    .AddErrorInfoProvider(opt => opt.ExposeExceptionStackTrace = true);
                
                //services.AddTransient<IGraphQLRequestDeserializer, GraphQLRequestDeserializer>();



                foreach (var query in builder.Definitions.Queries)
                {
                    var handlerType = typeof(GraphQueryHandler<,>).MakeGenericType(query.Value.QueryType, query.Value.ResultType);
                    var handlerFullType = typeof(IGraphQueryHandler<,>).MakeGenericType(query.Value.QueryType, query.Value.ResultType);
                    services.AddSingleton(handlerFullType, handlerType);
                    //services.AddSingleton(provider => (IGraphQueryHandler) provider.GetService(handlerFullType));
                }

            });

            UseExtensions.AddPostUseAction(app =>
            {
                            
                if (app.ApplicationServices.GetService(typeof(PlatformexGraphQlOptions)) is PlatformexGraphQlOptions optionsGraphQl)
                    app.UseGraphQL<ISchema>("/" + optionsGraphQl.BasePath.Trim('/'));

                if (app.ApplicationServices.GetService(typeof(PlatformexGraphQlConsoleOptions)) is PlatformexGraphQlConsoleOptions optionsConsole)
                    app.UseGraphQLPlayground("/" + optionsConsole.BasePath.Trim('/'));
            });
            return builder;
        }

        public static PlatformBuilder WithConsole(this PlatformBuilder builder, Action<PlatformexGraphQlConsoleOptions> builderOptions)
        {
            var options = new PlatformexGraphQlConsoleOptions("graphql-console");
            builderOptions(options);
            builder.AddConfigureServicesActions(services =>
            {
                services.AddSingleton(options);
                
            });

            return builder;
        }
    }
}
