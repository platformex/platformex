using System;
using Microsoft.Extensions.DependencyInjection;
using Platformex.Infrastructure;

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
    }
}