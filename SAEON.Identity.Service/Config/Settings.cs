using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAEON.Identity.Service.Data;
using SAEON.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Config
{
    public class User
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class Scope
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

    public class API
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<string> Claims { get; set; } = new List<string>();
        public List<string> Secrets { get; set; } = new List<string>();
        public List<Scope> Scopes { get; set; } = new List<Scope>();
    }

    public class Client
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GrantType { get; set; }
        public bool RequireConsent { get; set; }
        public bool RememberConsent { get; set; }
        public bool OfflineAccess { get; set; }
        public bool AccessTokensViaBrowser { get; set; }
        public int IdentityTokenLifetime { get; set; } = (int)new TimeSpan(14, 0, 0).TotalSeconds;
        public int AccessTokenLifetime { get; set; } = (int)new TimeSpan(7, 0, 0).TotalSeconds;
        public List<string> Secrets { get; set; } = new List<string>();
        public List<string> Scopes { get; set; } = new List<string>();
        public List<string> CorsOrigins { get; set; } = new List<string>();
        public List<string> RedirectURIs { get; set; } = new List<string>();
        public List<string> PostLogoutRedirectURIs { get; set; } = new List<string>();
    }

    public static class Settings
    {
        //private static readonly int IdentityServicePort = 44320;
        //private static readonly int ObservationsWebAPIPort = 54330;
        //private static readonly int ObservationsQuerySitePort = 54340;
        //private static readonly int ObservationsAdminSitePort = 54350;
        //private static readonly int EasiCATWebAPIPort = 55330;
        //private static readonly int EasiCATWebSitePort = 55340;

        public static List<string> Roles { get; private set; } = new List<string>();
        public static List<User> Users { get; private set; } = new List<User>();
        public static List<API> APIs { get; private set; } = new List<API>();

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource> {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email { Required=true},
                new IdentityResource {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources(IConfiguration config)
        {
            var result = new List<ApiResource>();
            var configuration = config.GetSection("IdentityService");
            var apis = new List<API>();
            configuration.GetSection("APIs").Bind(apis);
            foreach (var api in apis)
            {
                var apiResource = new ApiResource { Name = api.Name, DisplayName = api.DisplayName, Description = api.Description };
                foreach (var claim in api.Claims)
                {
                    apiResource.UserClaims.Add(claim);
                }
                foreach (var secret in api.Secrets)
                {
                    apiResource.ApiSecrets.Add(new Secret(secret.Sha256()));
                }
                foreach (var scope in api.Scopes)
                {
                    apiResource.Scopes.Add(new IdentityServer4.Models.Scope(scope.Name, scope.DisplayName));
                }
                result.Add(apiResource);
            }
            return result;
        }

        public static IEnumerable<IdentityServer4.Models.Client> GetClients(IConfiguration config)
        {
            var result = new List<IdentityServer4.Models.Client>();
            var configuration = config.GetSection("IdentityService");
            var clients = new List<Client>();
            configuration.GetSection("Clients").Bind(clients);
            foreach (var client in clients)
            {
                var isClient = new IdentityServer4.Models.Client
                {
                    ClientId = client.Id,
                    ClientName = client.Name,
                    RequireConsent = client.RequireConsent,
                    AllowRememberConsent = client.RememberConsent,
                    AllowOfflineAccess = client.OfflineAccess,
                    AllowAccessTokensViaBrowser = client.AccessTokensViaBrowser,
                    IdentityTokenLifetime = client.IdentityTokenLifetime,
                    AccessTokenLifetime = client.AccessTokenLifetime,
                };
                switch (client.GrantType)
                {
                    case "ClientCredentials":
                        isClient.AllowedGrantTypes = GrantTypes.ClientCredentials;
                        break;
                    case "Code":
                        isClient.AllowedGrantTypes = GrantTypes.Code;
                        break;
                    case "CodeAndClientCredentials":
                        isClient.AllowedGrantTypes = GrantTypes.CodeAndClientCredentials;
                        break;
                    case "Hybrid":
                        isClient.AllowedGrantTypes = GrantTypes.Hybrid;
                        break;
                    case "HybridAndClientCredentials":
                        isClient.AllowedGrantTypes = GrantTypes.HybridAndClientCredentials;
                        break;
                    case "Implicit":
                        isClient.AllowedGrantTypes = GrantTypes.Implicit;
                        break;
                    case "ImplicitAndClientCredentials":
                        isClient.AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials;
                        break;
                    case "ResourceOwnerPassword":
                        isClient.AllowedGrantTypes = GrantTypes.ResourceOwnerPassword;
                        break;
                    case "ResourceOwnerPasswordAndClientCredentials":
                        isClient.AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials;
                        break;
                    default:
                        Logging.Error("Unknown GrantType: {GranType}", client.GrantType);
                        continue;
                }
                foreach (var secret in client.Secrets)
                {
                    isClient.ClientSecrets.Add(new Secret(secret.Sha256()));
                }
                foreach (var scope in client.Scopes)
                {
                    isClient.AllowedScopes.Add(scope);
                }
                foreach (var corsOrigin in client.CorsOrigins)
                {
                    isClient.AllowedCorsOrigins.Add(corsOrigin);
                }
                foreach (var uri in client.RedirectURIs)
                {
                    isClient.RedirectUris.Add(uri);
                }
                foreach (var uri in client.PostLogoutRedirectURIs)
                {
                    isClient.PostLogoutRedirectUris.Add(uri);
                }
                result.Add(isClient);
            }
            return result;
        }

        public static List<string> GetRoles(IConfiguration config)
        {
            var configuration = config.GetSection("IdentityService");
            var roles = new List<string>();
            configuration.GetSection("Roles").Bind(roles);
            return roles;
        }

        public static List<User> GetUsers(IConfiguration config)
        {
            var configuration = config.GetSection("IdentityService");
            var users = new List<User>();
            configuration.GetSection("Users").Bind(users);
            return users;
        }

        internal static async Task InitializeDbAsync(IServiceProvider serviceProvider)
        {
            async Task AddUserAsync(User user, UserManager<SAEONUser> userManager)
            {
                using (Logging.MethodCall(typeof(Program), new ParameterList { { "FirstName", user.Name }, { "Surname", user.Surname }, { "Email", user.Email } }))
                {

                    if (await userManager.FindByNameAsync(user.Email) == null)
                    {
                        await userManager.CreateAsync(new SAEONUser { UserName = user.Email, Email = user.Email, FirstName = user.Name, Surname = user.Surname, EmailConfirmed = true }, user.Password);
                    }
                    var saeonUser = await userManager.FindByNameAsync(user.Email);
                    if ((saeonUser != null) && (user.Roles != null))
                        foreach (var role in user.Roles)
                        {
                            if (!await userManager.IsInRoleAsync(saeonUser, role))
                            {
                                await userManager.AddToRoleAsync(saeonUser, role);
                            }
                        }
                }
            }

            using (Logging.MethodCall(typeof(Program)))
            {
                Logging.Information("Seeding database");
                serviceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
                serviceProvider.GetRequiredService<SAEONDbContext>().Database.Migrate();
                var context = serviceProvider.GetRequiredService<ConfigurationDbContext>();
                var config = serviceProvider.GetRequiredService<IConfiguration>();

                foreach (var resource in Settings.GetIdentityResources())
                {
                    if (!await context.IdentityResources.AnyAsync(i => i.Name == resource.Name))
                    {
                        await context.IdentityResources.AddAsync(resource.ToEntity());
                    }
                }
                context.SaveChanges();

                foreach (var resource in Settings.GetApiResources(config))
                {
                    if (!await context.ApiResources.AnyAsync(i => i.Name == resource.Name))
                    {
                        await context.ApiResources.AddAsync(resource.ToEntity());
                    }
                }
                context.SaveChanges();

                foreach (var client in Settings.GetClients(config))
                {
                    if (!await context.Clients.AnyAsync(i => i.ClientName == client.ClientName))
                    {
                        await context.Clients.AddAsync(client.ToEntity());
                    }
                }
                context.SaveChanges();

                var roleManager = serviceProvider.GetRequiredService<RoleManager<SAEONRole>>();
                foreach (var role in Settings.GetRoles(config))
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var identityRole = new SAEONRole { Name = role };
                        await roleManager.CreateAsync(identityRole);
                        await roleManager.AddClaimAsync(identityRole, new Claim(ClaimTypes.Role, role));
                    }
                }
                var userManager = serviceProvider.GetRequiredService<UserManager<SAEONUser>>();
                foreach (var user in Settings.GetUsers(config))
                {
                    await AddUserAsync(user, userManager);
                }
            }
        }

        //public static IEnumerable<Client> GetClients()
        //{
        //    return new List<Client> {
        //        //new Client
        //        //{
        //        //    ClientId = "SAEON.Observations.WebAPI",
        //        //    ClientName = "SAEON Observations WebAPI",
        //        //    AllowedGrantTypes = GrantTypes.ClientCredentials,
        //        //    IdentityTokenLifetime = (int)new TimeSpan(14, 0, 0).TotalSeconds,
        //        //    AccessTokenLifetime = (int)new TimeSpan(7, 0, 0).TotalSeconds,
        //        //    ClientSecrets = new List<Secret> { new Secret("81g5wyGSC89a".Sha256()) },
        //        //    AllowedScopes = new List<string> { "SAEON.Observations.WebAPI" },
        //        //    AllowedCorsOrigins = new List<string>
        //        //    {
        //        //        $"http://localhost:{QuerySitePort}",
        //        //        "http://observations.saeon.ac.za",
        //        //    },
        //        //},
        //        new Client {
        //            ClientId = "SAEON.Observations.QuerySite",
        //            ClientName = "SAEON Observations QuerySite",
        //            AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
        //            ClientSecrets = new List<Secret> { new Secret("It6fWPU5J708".Sha256()) },
        //            IdentityTokenLifetime = (int)new TimeSpan(14, 0, 0).TotalSeconds,
        //            AccessTokenLifetime = (int)new TimeSpan(7, 0, 0).TotalSeconds,
        //            RequireConsent = true,
        //            AllowRememberConsent = true,
        //            RedirectUris = new List<string>
        //            {
        //                $"http://localhost:{ObservationsQuerySitePort}",
        //                "http://observations.saeon.ac.za",
        //            },
        //            PostLogoutRedirectUris = new List<string>
        //            {
        //                $"http://localhost:{ObservationsQuerySitePort}",
        //                "http://observations.saeon.ac.za",
        //            },
        //            AllowedScopes = new List<string>
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                IdentityServerConstants.StandardScopes.Email,
        //                "SAEON.Observations.WebAPI"
        //            },
        //            AllowedCorsOrigins = new List<string>
        //            {
        //                $"http://localhost:{ObservationsQuerySitePort}",
        //                "http://observations.saeon.ac.za",
        //            },
        //            AllowOfflineAccess = true,
        //            AllowAccessTokensViaBrowser = true
        //        },
        //        new Client {
        //            ClientId = "SAEON.Observations.AdminSite",
        //            ClientName = "SAEON Observations AdminSite",
        //            AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
        //            IdentityTokenLifetime = (int)new TimeSpan(14, 0, 0).TotalSeconds,
        //            AccessTokenLifetime = (int)new TimeSpan(7, 0, 0).TotalSeconds,
        //            RequireConsent = true,
        //            AllowRememberConsent = true,
        //            RedirectUris = new List<string>
        //            {
        //                $"http://localhost:{ObservationsAdminSitePort}",
        //                "http://observations.saeon.ac.za",
        //            },
        //            PostLogoutRedirectUris = new List<string>
        //            {
        //                $"http://localhost:{ObservationsAdminSitePort}",
        //                "http://observations.saeon.ac.za",
        //            },
        //            AllowedScopes = new List<string>
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                IdentityServerConstants.StandardScopes.Email,
        //                "SAEON.Observations.WebAPI"
        //            },
        //            AllowedCorsOrigins = new List<string>
        //            {
        //                $"http://localhost:{ObservationsQuerySitePort}",
        //                "http://observations.saeon.ac.za",
        //            },
        //            AllowOfflineAccess = true,
        //            AllowAccessTokensViaBrowser = true
        //        },
        //        new Client {
        //            ClientId = "SAEON.EasiCAT.WebSite",
        //            ClientName = "SAEON EasiCAT WebSite",
        //            AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
        //            ClientSecrets = new List<Secret> { new Secret("It6fWPU5J708".Sha256()) },
        //            IdentityTokenLifetime = (int)new TimeSpan(14, 0, 0).TotalSeconds,
        //            AccessTokenLifetime = (int)new TimeSpan(7, 0, 0).TotalSeconds,
        //            RequireConsent = true,
        //            AllowRememberConsent = true,
        //            RedirectUris = new List<string>
        //            {
        //                $"http://localhost:{EasiCATWebSitePort}",
        //                //"http://observations.saeon.ac.za",
        //            },
        //            PostLogoutRedirectUris = new List<string>
        //            {
        //                $"http://localhost:{EasiCATWebSitePort}",
        //                //"http://observations.saeon.ac.za",
        //            },
        //            AllowedScopes = new List<string>
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                IdentityServerConstants.StandardScopes.Email,
        //                "SAEON.EasiCAT.WebAPI"
        //            },
        //            AllowedCorsOrigins = new List<string>
        //            {
        //                $"http://localhost:{EasiCATWebSitePort}",
        //                //"http://observations.saeon.ac.za",
        //            },
        //            AllowOfflineAccess = true,
        //            AllowAccessTokensViaBrowser = true
        //        },
        //        new Client
        //        {
        //            ClientId = "Postman",
        //            ClientName = "Postman client",
        //            AllowedGrantTypes = GrantTypes.ClientCredentials,
        //            IdentityTokenLifetime = (int)new TimeSpan(1, 0, 0).TotalSeconds,
        //            AccessTokenLifetime = (int)new TimeSpan(1, 0, 0).TotalSeconds,
        //            RequireConsent = true,
        //            AllowRememberConsent = true,
        //            ClientSecrets = new List<Secret> { new Secret("Postman".Sha256()) },
        //            AllowedScopes = new List<string>
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Email,
        //                "SAEON.Observations.WebAPI"
        //            },
        //            RedirectUris = new List<string>()
        //            {
        //                "https://www.getpostman.com/oauth2/callback"
        //            },
        //        },
        //        new Client
        //        {
        //            ClientId = "Swagger",
        //            ClientName = "Swagger Client",
        //            AllowedGrantTypes = GrantTypes.Implicit,
        //            IdentityTokenLifetime = (int)new TimeSpan(1, 0, 0).TotalSeconds,
        //            AccessTokenLifetime = (int)new TimeSpan(1, 0, 0).TotalSeconds,
        //            RequireConsent = true,
        //            AllowRememberConsent = true,
        //            ClientSecrets = new List<Secret> { new Secret("Swagger".Sha256()) },
        //            AllowedScopes = new List<string>
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Email,
        //                "SAEON.Observations.WebAPI"
        //            },
        //            RedirectUris = new List<string>
        //            {
        //                $"http://localhost:{ObservationsWebAPIPort}/swagger/ui/o2c-html",
        //                "http://observationsapi.saeon.ac.za/swagger/ui/o2c-html",
        //            },
        //            AllowAccessTokensViaBrowser = true
        //        },
        //    };
    }
}
