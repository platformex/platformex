using System;
using IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Platformex.Infrastructure;

namespace Platformex.Web.IdentityServer
{
    public sealed class PlatformexIdentityOptions
    {
        public string IdentityServerUri { get; set; }
    }

    public static class BuilderExtensions
    {
        public static PlatformBuilder ConfigureIdentity(this PlatformBuilder builder, Action<PlatformexIdentityOptions> optionsBuilder)
        {
            var opt = new PlatformexIdentityOptions{ IdentityServerUri = "https://localhost:5000" };

            builder.AddConfigureServicesActions(services =>
            {
                optionsBuilder?.Invoke(opt);

                //services.AddAuthentication("Bearer")
                //    .AddIdentityServerAuthentication("Bearer", options =>
                //    {
                //        // required audience of access tokens
                //        options.ApiName = "platformex";

                //        // auth server base endpoint (this will be used to search for disco doc)
                //        options.Authority = opt.IdentityServerUri;
                //    });
                services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.Authority = opt.IdentityServerUri;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            NameClaimType = JwtClaimTypes.Subject,
                            RoleClaimType = JwtClaimTypes.Role
                        };
                    });

            });
            UseExtensions.AddPreUseAction(app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
            });
            return builder;
        }

    }
}
