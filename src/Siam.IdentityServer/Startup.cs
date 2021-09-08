// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Siam.IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            var builder = services.AddIdentityServer(options =>
                {
                    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                    options.EmitStaticAudienceClaim = true;
                })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients);
                AddTestUsers(builder,new List<TestUser>
                {
                    new()
                    {
                        IsActive = true,
                        Username = "user",
                        Password = "Passw0rd",
                        SubjectId = Guid.NewGuid().ToString(),
                        Claims = new List<Claim>
                        {
                            new(JwtClaimTypes.Id, "user"),
                            new(JwtClaimTypes.Name, "Иванов Иван")
                        }
                    }
                });

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();
        }
        public static IIdentityServerBuilder AddTestUsers(IIdentityServerBuilder builder, List<TestUser> users)
        {
            builder.Services.AddSingleton(new TestUserStore(users));
            builder.AddProfileService<MyProfileService>();
            builder.AddResourceOwnerValidator<TestUserResourceOwnerPasswordValidator>();

            return builder;
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // uncomment if you want to add MVC
            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseIdentityServer();

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }

    public class MyProfileService : TestUserProfileService
    {
        public MyProfileService(TestUserStore users, ILogger<TestUserProfileService> logger) : base(users, logger)
        {
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var scopes = Config.IdentityResources.ToList();
            var requestClimes = new List<string>();
            foreach (var parsedScope in context.RequestedResources.ParsedScopes)
            {
                var scope = scopes.FirstOrDefault(i => i.Name == parsedScope.ParsedName);
                if (scope != null)
                    requestClimes.AddRange(scope.UserClaims);

            }

            context.RequestedClaimTypes = requestClimes;
            await base.GetProfileDataAsync(context);
        }
    }
}
