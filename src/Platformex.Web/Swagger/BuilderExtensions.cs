using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Platformex.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Platformex.Web.Swagger
{
    public sealed class PlatformexOpenApiOptions
    {
        public PlatformexOpenApiOptions(string url, string name)
        {
            Url = url;
            Name = name;
        }

        public string Url { get; set; }
        public string Name { get; set; }
    }

    public static class BuilderExtensions
    {
        public static PlatformBuilder WithOpenApi(this PlatformBuilder builder, Action<PlatformexOpenApiOptions> optionsBuilder)
        {
            var options = new PlatformexOpenApiOptions("swagger", Assembly.GetEntryAssembly()?.GetName().Name);
            optionsBuilder(options);
            builder.AddConfigureServicesActions(services =>
            {
                services.AddSingleton(options);
                
                services.TryAdd(ServiceDescriptor
                    .Transient<IApiDescriptionGroupCollectionProvider,
                        CommandsApiDescriptionGroupCollectionProvider>());
                            
                services.AddSingleton<IApiDescriptionGroupCollectionProvider,
                    CommandsApiDescriptionGroupCollectionProvider>();

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo {Title = options.Name + " API", Version = "v1"});
                    c.OperationFilter<DescriptionFilter>();
                    c.OperationFilter<AuthorizeCheckOperationFilter>();
                    c.SchemaFilter<ReadOnlyFilter>();
                    c.CustomSchemaIds(i => i.FullName);
                    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("https://localhost:5000/connect/authorize"),
                                TokenUrl = new Uri("https://localhost:5000/connect/token"),
                                Scopes = new Dictionary<string, string>
                                {
                                    { "openid", "User Profile" },
                                    { "platformex", "Platformex API - full access"}
                                }
                            }
                        }
                    });
                    c.DocInclusionPredicate((docName, apiDesc) 
                        => apiDesc.TryGetMethodInfo(out _) && apiDesc.HttpMethod != null);
                    
                    /*var basePath = AppDomain.CurrentDomain.BaseDirectory;
                    var files = Directory.GetFiles(basePath, "*.xml");
                    foreach (var file in files)
                    {
                        c.IncludeXmlComments(file);
                    }*/

                });
            });
            UseExtensions.AddPreUseAction(app =>
            {
                app.UseSwagger();

                var apiOptions = app.ApplicationServices.GetRequiredService<PlatformexOpenApiOptions>();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/" + apiOptions.Url.Trim('/') + "/v1/swagger.json", apiOptions.Name);

                    c.OAuthClientId("swagger");
                    c.OAuthAppName("Platformex Open API");
                    c.OAuthUsePkce();
                });

            });
            return builder;
        }
    }

    public class DescriptionFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor desc) return;

            var actionType = desc.ControllerTypeInfo.AsType();
            var isCommand = actionType.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(ICommand<>));
                
            var isQuery = actionType.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(ICommand<>));

            if (typeof(IService).IsAssignableFrom(actionType))
            {
                operation.Summary = desc.MethodInfo.GetCustomAttribute<DescriptionAttribute>() != null ? desc.MethodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description : null;
            }

            if (isCommand || isQuery)
            {
                operation.Summary = actionType.GetCustomAttribute<DescriptionAttribute>() != null ? actionType.GetCustomAttribute<DescriptionAttribute>()?.Description : null;
            }
        }
    }
    public class ReadOnlyFilter : ISchemaFilter
    {

        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (model.Properties == null)
            {
                return;
            }

            foreach (var schemaProperty in model.Properties)
            {
                var property = context.Type.GetProperty(schemaProperty.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    schemaProperty.Value.ReadOnly = false;
                }
            }
        }
    }
}
