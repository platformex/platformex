// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityModel;
using IdentityServer4;

namespace Siam.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new[]
            { 
                new IdentityResource("openid", new []{JwtClaimTypes.Id, JwtClaimTypes.Subject})
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("platformex", "Platformex API")
            };

        public static IEnumerable<Client> Clients =>
            new[]
            {
                new Client
                {
                    ClientId = "swagger",
                    ClientName = "Swagger UI for Platformex",
                    //ClientSecrets = {new Secret("Passw0rd")}, // change me!
                    RequireConsent = true,
                    AlwaysIncludeUserClaimsInIdToken = true,

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris = {"https://localhost:44317/swagger/oauth2-redirect.html"},
                    AllowedCorsOrigins = {"https://localhost:44317"},
                    AllowedScopes = 
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "platformex"
                    }
                }
            };

    }
}