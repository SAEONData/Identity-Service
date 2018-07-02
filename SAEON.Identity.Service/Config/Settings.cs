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
using System.Security.Claims;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Config
{


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
            var _logic = new ConfigControllerLogic();

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
                    AllowedGrantTypes = _logic.StringToGrantType(client.GrantType)
                };

                //switch (client.GrantType)
                //{
                //    case "ClientCredentials":
                //        isClient.AllowedGrantTypes = GrantTypes.ClientCredentials;
                //        break;
                //    case "Code":
                //        isClient.AllowedGrantTypes = GrantTypes.Code;
                //        break;
                //    case "CodeAndClientCredentials":
                //        isClient.AllowedGrantTypes = GrantTypes.CodeAndClientCredentials;
                //        break;
                //    case "Hybrid":
                //        isClient.AllowedGrantTypes = GrantTypes.Hybrid;
                //        break;
                //    case "HybridAndClientCredentials":
                //        isClient.AllowedGrantTypes = GrantTypes.HybridAndClientCredentials;
                //        break;
                //    case "Implicit":
                //        isClient.AllowedGrantTypes = GrantTypes.Implicit;
                //        break;
                //    case "ImplicitAndClientCredentials":
                //        isClient.AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials;
                //        break;
                //    case "ResourceOwnerPassword":
                //        isClient.AllowedGrantTypes = GrantTypes.ResourceOwnerPassword;
                //        break;
                //    case "ResourceOwnerPasswordAndClientCredentials":
                //        isClient.AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials;
                //        break;
                //    default:
                //        Logging.Error("Unknown GrantType: {GranType}", client.GrantType);
                //        continue;
                //}

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
    }
}
