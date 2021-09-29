// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Chuech.ProjectSce.Identity
{
    public static class IdentityConfiguration
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new("core_identity", new[] {ChuechClaimTypes.PublicId})
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new("core.full", new[] {ChuechClaimTypes.PublicId})
            };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("core", "Core API")
            {
                ApiSecrets =
                {
                    new Secret("whatever_lul".Sha256()) // TODO: Security concern, wtf is this secret?
                },
                Scopes = {"core.full"},
                UserClaims = {ChuechClaimTypes.PublicId, JwtClaimTypes.Name, JwtClaimTypes.PreferredUserName}
            }
        };

        public static IEnumerable<Client> GetClients(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var spaClient = configuration.GetValue<string>("SpaClient");
            var realConditions = configuration.GetValue("RealConditionsTokens", false);

            return new[]
            {
                new Client
                {
                    ClientId = "spa",
                    ClientName = "Project SCE",
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = true,

                    AccessTokenType = AccessTokenType.Reference,

                    RedirectUris = {$"{spaClient}/authcallback", $"{spaClient}/silentrenew"},
                    PostLogoutRedirectUris = {$"{spaClient}/"},
                    AllowedCorsOrigins = {spaClient},

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "core.full",
                        "core_identity"
                    },

                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    
                    // Use a longer access token lifetime for development
                    // so we can test the API without having to log in every hour.
                    AccessTokenLifetime = environment.IsDevelopment() && !realConditions ?
                        (int) TimeSpan.FromDays(7).TotalSeconds :
                        (int) TimeSpan.FromHours(1).TotalSeconds
                }
            };
        }
    }
}