﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platformex.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Platformex.Web
{
    public static class BuilderExtensions
    {
        public static PlatformBuilder ConfigureWebApi(this PlatformBuilder builder, Action<PlatformexWebApiOptions> optionsBuilder)
        {
            var options = new PlatformexWebApiOptions("api");
            optionsBuilder(options);
            builder.AddConfigureServicesActions(collection =>
            {
                collection.AddSingleton(options);
            });
            return builder;
        }
        public static PlatformBuilder ConfigureWebApi(this PlatformBuilder builder)
        {
            builder.ConfigureWebApi(_ => { });
            return builder;
        }
        public static PlatformBuilder ConfigureServices(this PlatformBuilder builder, Action<IServiceCollection, IConfiguration> configAction)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            builder.AddConfigureServicesActions(collection =>
            {
                configAction(collection, configuration);
            });


            return builder;
        }

        public static PlatformBuilder StartupAction(this PlatformBuilder builder, Func<IPlatform, Task> func)
        {
            builder.AddStartupActions(async provider =>
            {
                var platform = provider.GetService<IPlatform>();
                await func(platform);
            });
            return builder;
        }


    }
}