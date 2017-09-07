using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service
{
    internal class Config
    {
        private static readonly int WebAPIPort = 54330;
        private static readonly int QuerySitePort = 54340;
        private static readonly int AdminSitePort = 54350;

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource> {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource> {
                new ApiResource {
                    Name = "SAEON.Observations.WebAPI",
                    DisplayName = "SAEON Observations WebAPI",
                    Description = "The SAEON Observations WebAPI",
                    UserClaims = new List<string> {"role", JwtClaimTypes.ClientId},
                    ApiSecrets = new List<Secret> {new Secret("KIxc0hO9AJ7Q".Sha256())},
                    Scopes = new List<Scope> {
                        new Scope("SAEON.Observations.WebAPI","SAEON Observations WebAPI")
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client> {
                new Client
                {
                    ClientId = "SAEON.Observations.WebAPI",
                    ClientName = "SAEON Observations WebAPI",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    IdentityTokenLifetime = (int)new TimeSpan(14, 0, 0).TotalSeconds,
                    AccessTokenLifetime = (int)new TimeSpan(7, 0, 0).TotalSeconds,
                    ClientSecrets = new List<Secret> { new Secret("81g5wyGSC89a".Sha256()) },
                    AllowedScopes = new List<string> { "SAEON.Observations.WebAPI" },
                },
                new Client {
                    ClientId = "SAEON.Observations.QuerySite",
                    ClientName = "SAEON Observations QuerySite",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    ClientSecrets = new List<Secret> { new Secret("It6fWPU5J708".Sha256()) },
                    IdentityTokenLifetime = (int)new TimeSpan(14, 0, 0).TotalSeconds,
                    AccessTokenLifetime = (int)new TimeSpan(7, 0, 0).TotalSeconds,
                    RequireConsent = true,
                    AllowRememberConsent = true,
                    RedirectUris = new List<string>
                    {
                        $"http://localhost:{QuerySitePort}",
                        "http://observations.saeon.ac.za",
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        $"http://localhost:{QuerySitePort}",
                        "http://observations.saeon.ac.za",
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "SAEON.Observations.WebAPI"
                    },
                    AllowOfflineAccess = true
                },
                new Client {
                    ClientId = "SAEON.Observations.AdminSite",
                    ClientName = "SAEON Observations AdminSite",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    IdentityTokenLifetime = (int)new TimeSpan(14, 0, 0).TotalSeconds,
                    AccessTokenLifetime = (int)new TimeSpan(7, 0, 0).TotalSeconds,
                    RequireConsent = true,
                    AllowRememberConsent = true,
                    RedirectUris = new List<string>
                    {
                        $"http://localhost:{AdminSitePort}",
                        "http://observations.saeon.ac.za",
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        $"http://localhost:{AdminSitePort}",
                        "http://observations.saeon.ac.za",
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "SAEON.Observations.WebAPI"
                    },
                    AllowOfflineAccess = true
                },
                new Client
                {
                    ClientId = "Postman",
                    ClientName = "Postman client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    IdentityTokenLifetime = (int)new TimeSpan(1, 0, 0).TotalSeconds,
                    AccessTokenLifetime = (int)new TimeSpan(1, 0, 0).TotalSeconds,
                    RequireConsent = true,
                    AllowRememberConsent = true,
                    ClientSecrets = new List<Secret> { new Secret("Postman".Sha256()) },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        "SAEON.Observations.WebAPI"
                    },
                    RedirectUris = new List<string>()
                    {
                        "https://www.getpostman.com/oauth2/callback"
                    },
                },
                new Client
                {
                    ClientId = "Swagger",
                    ClientName = "Swagger Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    IdentityTokenLifetime = (int)new TimeSpan(1, 0, 0).TotalSeconds,
                    AccessTokenLifetime = (int)new TimeSpan(1, 0, 0).TotalSeconds,
                    RequireConsent = true,
                    AllowRememberConsent = true,
                    ClientSecrets = new List<Secret> { new Secret("Swagger".Sha256()) },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        "SAEON.Observations.WebAPI"
                    },
                    RedirectUris = new List<string>
                    {
                        $"http://localhost:{WebAPIPort}/swagger/ui/o2c-html",
                        "http://observationsapi.saeon.ac.za/swagger/ui/o2c-html",
                    },
                    AllowAccessTokensViaBrowser = true
                },
            };
        }
    }
}
