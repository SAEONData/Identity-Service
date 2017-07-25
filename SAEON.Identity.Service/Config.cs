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
                    UserClaims = new List<string> {"role"},
                    ApiSecrets = new List<Secret> {new Secret("81g5wyGSC89a".Sha256())},
                    Scopes = new List<Scope> {
                        new Scope("SAEON.Observations.WebAPI")
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client> {
                new Client {
                    ClientId = "SAEON.Observations.QuerySite",
                    ClientName = "SAEON Observations QuerySite",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    RequireConsent = true,
                    AllowRememberConsent = true,
                    RedirectUris = new List<string> {
                        "http://localhost:54321/signin-oidc",
                        "https://localhost:44321/signin-oidc",
                        "http://observations.saeon.ac.za/signin-oidc",
                        "https://observations.saeon.ac.za/signin-oidc",
                        "http://observations.nimbusservices.co.za/signin-oidc",
                        "https://observations.nimbusservices.co.za/signin-oidc",
                        "http://observations.saeon.nimbusservices.co.za/signin-oidc",
                        "https://observations.saeon.nimbusservices.co.za/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string> {
                        "http://localhost:54321/signout-callback-oidc",
                        "https://localhost:44321/signout-callback-oidc",
                        "http://observations.saeon.ac.za/signout-callback-oidc",
                        "https://observations.saeon.ac.za/signout-callback-oidc",
                        "http://observations.nimbusservices.co.za/signout-callback-oidc",
                        "https://observations.nimbusservices.co.za/signout-callback-oidc",
                        "http://observations.saeon.nimbusservices.co.za/signout-callback-oidc",
                        "https://observations.saeon.nimbusservices.co.za/signout-callback-oidc",
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "SAEON.Observations.WebAPI"
                    },
                    AllowOfflineAccess = true
                }
            };
        }
    }
}
