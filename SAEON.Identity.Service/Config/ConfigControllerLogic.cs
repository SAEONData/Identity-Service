using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAEON.Identity.Service.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Config
{
    internal class ConfigControllerLogic
    {
        internal Client GetClientResource(int Id, out Exception error)
        {
            Client clientResource = new Client();

            var data = GetClientResources(out error).FirstOrDefault(x => x.Id == Id);
            var dataModel = data.ToModel();
            if (data != null && error == null)
            {
                try
                {
                    clientResource = new Client()
                    {
                        dbid = data.Id,
                        Name = dataModel.ClientId,
                        DisplayName = dataModel.ClientName,
                        IdentityTokenLifetime = dataModel.IdentityTokenLifetime,
                        AccessTokenLifetime = dataModel.AccessTokenLifetime,
                        GrantType = GrantTypeToString(dataModel.AllowedGrantTypes),
                        Secrets = data.ClientSecrets.Select(x => x.Value).ToList(),
                        Scopes = data.AllowedScopes.Select(x => x.Scope).ToList(),
                        CorsOrigins = data.AllowedCorsOrigins.Select(x => x.Origin).ToList(),
                        RedirectURIs = data.RedirectUris.Select(x => x.RedirectUri).ToList(),
                        PostLogoutRedirectURIs = data.PostLogoutRedirectUris.Select(x => x.PostLogoutRedirectUri).ToList(),
                        RequireConsent = data.RequireConsent,
                        RememberConsent = data.AllowRememberConsent,
                        OfflineAccess = data.AllowOfflineAccess,
                        AccessTokensViaBrowser = data.AllowAccessTokensViaBrowser
                    };
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            return clientResource;
        }

        internal List<IdentityServer4.EntityFramework.Entities.Client> GetClientResources(out Exception error)
        {
            var clientResources = new List<IdentityServer4.EntityFramework.Entities.Client>();
            error = null;

            var host = Program.host;
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                using (var context = serviceProvider.GetRequiredService<ConfigurationDbContext>())
                {
                    try
                    {
                        clientResources = context.Clients
                            .Include(c => c.AllowedGrantTypes)
                            .Include(c => c.ClientSecrets)
                            .Include(c => c.AllowedScopes)
                            .Include(c => c.AllowedCorsOrigins)
                            .Include(c => c.RedirectUris)
                            .Include(c => c.PostLogoutRedirectUris)
                            .OrderBy(c => c.ClientId).ToList();

                        //TEST
                        throw new Exception("test", new Exception("test_inner", new Exception("test_inner_inner")));
                        //TEST
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to get Client Resources from DB.");

                        error = ex;
                    }

                }
            }

            return clientResources;
        }

        internal IdentityServer4.EntityFramework.Entities.Client BuildClient(Client client)
        {
            var isClient = new IdentityServer4.Models.Client
            {
                ClientId = client.Name,
                ClientName = client.DisplayName,
                RequireConsent = client.RequireConsent,
                AllowRememberConsent = client.RememberConsent,
                AllowOfflineAccess = client.OfflineAccess,
                AllowAccessTokensViaBrowser = client.AccessTokensViaBrowser,
                IdentityTokenLifetime = client.IdentityTokenLifetime,
                AccessTokenLifetime = client.AccessTokenLifetime,
                AllowedGrantTypes = StringToGrantType(client.GrantType)
            };

            if (client.NewSecrets != null)
            {
                if (!client.Secrets.OrderBy(x => x).SequenceEqual(client.NewSecrets.Select(x => x.Sha256()).OrderBy(x => x)))
                {
                    //Replace existing secrets is any values changed
                    foreach (var secret in client.NewSecrets)
                    {
                        isClient.ClientSecrets.Add(new IdentityServer4.Models.Secret(secret.Sha256()));
                    }
                }
                else
                {
                    //Keep existing secrets
                    foreach (var secret in client.Secrets)
                    {
                        isClient.ClientSecrets.Add(new IdentityServer4.Models.Secret(secret));
                    }
                }

            }

            if (client.Scopes != null)
            {
                foreach (var scope in client.Scopes)
                {
                    isClient.AllowedScopes.Add(scope);
                }
            }

            if (client.CorsOrigins != null)
            {
                foreach (var corsOrigin in client.CorsOrigins)
                {
                    isClient.AllowedCorsOrigins.Add(corsOrigin);
                }
            }

            if (client.RedirectURIs != null)
            {
                foreach (var uri in client.RedirectURIs)
                {
                    isClient.RedirectUris.Add(uri);
                }
            }

            if (client.PostLogoutRedirectURIs != null)
            {
                foreach (var uri in client.PostLogoutRedirectURIs)
                {
                    isClient.PostLogoutRedirectUris.Add(uri);
                }
            }

            var isEntClient = isClient.ToEntity();
            isEntClient.Id = client.dbid;

            return isEntClient;
        }

        internal ICollection<string> StringToGrantType(string value)
        {
            switch (value.ToLower())
            {
                case "clientcredentials":
                    return GrantTypes.ClientCredentials;
                case "code":
                    return GrantTypes.Code;
                case "codeandclientcredentials":
                    return GrantTypes.CodeAndClientCredentials;
                case "hybrid":
                    return GrantTypes.Hybrid;
                case "hybridandclientcredentials":
                    return GrantTypes.HybridAndClientCredentials;
                case "implicit":
                    return GrantTypes.Implicit;
                case "implicitandclientcredentials":
                    return GrantTypes.ImplicitAndClientCredentials;
                case "resourceownerpassword":
                    return GrantTypes.ResourceOwnerPassword;
                case "resourceownerpasswordandclientcredentials":
                    return GrantTypes.ResourceOwnerPasswordAndClientCredentials;
                default:
                    return new List<string>();
            }
        }

        internal string GrantTypeToString(ICollection<string> value)
        {
            if (value.OrderBy(x => x).SequenceEqual(GrantTypes.ClientCredentials.OrderBy(x => x)))
                return "ClientCredentials";
            else if (value.OrderBy(x => x).SequenceEqual(GrantTypes.Code.OrderBy(x => x)))
                return "Code";
            else if (value.OrderBy(x => x).SequenceEqual(GrantTypes.CodeAndClientCredentials.OrderBy(x => x)))
                return "CodeAndClientCredentials";
            else if (value.OrderBy(x => x).SequenceEqual(GrantTypes.Hybrid.OrderBy(x => x)))
                return "Hybrid";
            else if (value.OrderBy(x => x).SequenceEqual(GrantTypes.HybridAndClientCredentials.OrderBy(x => x)))
                return "HybridAndClientCredentials";
            else if (value.OrderBy(x => x).SequenceEqual(GrantTypes.Implicit.OrderBy(x => x)))
                return "Implicit";
            else if (value.OrderBy(x => x).SequenceEqual(GrantTypes.ImplicitAndClientCredentials.OrderBy(x => x)))
                return "ImplicitAndClientCredentials";
            else if (value.OrderBy(x => x).SequenceEqual(GrantTypes.ResourceOwnerPassword.OrderBy(x => x)))
                return "ResourceOwnerPassword";
            else if (value.OrderBy(x => x).SequenceEqual(GrantTypes.ResourceOwnerPasswordAndClientCredentials.OrderBy(x => x)))
                return "ResourceOwnerPasswordAndClientCredentials";
            else
                return "";
        }

        internal bool SaveClient(IdentityServer4.EntityFramework.Entities.Client client)
        {
            bool result = false;

            try
            {
                var host = Program.host;
                using (var scope = host.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    using (var context = serviceProvider.GetRequiredService<ConfigurationDbContext>())
                    {
                        try
                        {
                            var dbClient = context.Clients
                                .Include(c => c.AllowedGrantTypes)
                                .Include(c => c.ClientSecrets)
                                .Include(c => c.AllowedScopes)
                                .Include(c => c.AllowedCorsOrigins)
                                .Include(c => c.RedirectUris)
                                .Include(c => c.PostLogoutRedirectUris)
                                .FirstOrDefault(x => x.Id == client.Id);

                            if (dbClient != null)
                            {
                                //UPDATE
                                dbClient.ClientId = client.ClientId;
                                dbClient.ClientName = client.ClientName;
                                dbClient.IdentityTokenLifetime = client.IdentityTokenLifetime;
                                dbClient.AccessTokenLifetime = client.AccessTokenLifetime;
                                dbClient.AllowedGrantTypes = client.AllowedGrantTypes;
                                dbClient.ClientSecrets = client.ClientSecrets;
                                dbClient.AllowedScopes = client.AllowedScopes;
                                dbClient.AllowedCorsOrigins = client.AllowedCorsOrigins;
                                dbClient.RedirectUris = client.RedirectUris;
                                dbClient.PostLogoutRedirectUris = client.PostLogoutRedirectUris;
                                dbClient.RequireConsent = client.RequireConsent;
                                dbClient.AllowRememberConsent = client.AllowRememberConsent;
                                dbClient.AllowOfflineAccess = client.AllowOfflineAccess;
                                dbClient.AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser;
                            }
                            else
                            {
                                //ADD
                                context.Clients.Add(client);
                            }

                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                            logger.LogError(ex, "Unabled to save ClientResources to DB.");

                            throw ex;
                        }
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        internal bool DeleteClient(int Id, out Exception error)
        {
            bool result = false;
            error = null;

            var host = Program.host;
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                using (var context = serviceProvider.GetRequiredService<ConfigurationDbContext>())
                {
                    try
                    {
                        var client = context.Clients
                            .Include(c => c.AllowedGrantTypes)
                            .Include(c => c.ClientSecrets)
                            .Include(c => c.AllowedScopes)
                            .Include(c => c.AllowedCorsOrigins)
                            .Include(c => c.RedirectUris)
                            .Include(c => c.PostLogoutRedirectUris)
                            .FirstOrDefault(x => x.Id == Id);

                        if (client != null)
                        {
                            context.Clients.Remove(client);
                            context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to delete ClientResource from DB.");

                        error = ex;
                    }

                }
            }

            return result;
        }

        internal List<IdentityServer4.EntityFramework.Entities.ApiResource> GetApiResources(out Exception error)
        {
            var apiResources = new List<IdentityServer4.EntityFramework.Entities.ApiResource>();
            error = null;

            var host = Program.host;
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                using (var context = serviceProvider.GetRequiredService<ConfigurationDbContext>())
                {
                    try
                    {
                        apiResources = context.ApiResources
                            .Include(x => x.UserClaims)
                            .Include(x => x.Secrets)
                            .Include(x => x.Scopes)
                            .OrderBy(c => c.Name).ToList();
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to get API Resources from DB.");

                        error = ex;
                    }

                }
            }

            return apiResources;
        }

        internal API GetApiResource(int Id, out Exception error)
        {
            API apiResource = new API();
            error = null;

            var data = GetApiResources(out error).FirstOrDefault(x => x.Id == Id);
            var dataModel = data.ToModel();
            if (data != null && error == null)
            {
                try
                {
                    apiResource = new API()
                    {
                        dbid = data.Id,
                        Name = dataModel.Name,
                        DisplayName = dataModel.DisplayName,
                        Description = dataModel.Description,
                        Claims = dataModel.UserClaims.ToList(),
                        Secrets = dataModel.ApiSecrets.Select(x => x.Value).ToList(),
                        Scopes = dataModel.Scopes.Select(x => new Scope() { Name = x.Name, DisplayName = x.DisplayName }).ToList()
                    };
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            return apiResource;
        }

        internal IdentityServer4.EntityFramework.Entities.ApiResource BuildApi(API api)
        {
            var isApi = new IdentityServer4.Models.ApiResource
            {
                Name = api.Name,
                DisplayName = api.DisplayName,
                Description = api.Description
            };

            if (api.Claims != null)
            {
                foreach (var claim in api.Claims)
                {
                    isApi.UserClaims.Add(claim);
                }
            }

            if (api.NewSecrets != null)
            {
                if (!api.Secrets.OrderBy(x => x).SequenceEqual(api.NewSecrets.Select(x => x.Sha256()).OrderBy(x => x)))
                {
                    //Replace existing secrets is any values changed
                    foreach (var secret in api.NewSecrets)
                    {
                        isApi.ApiSecrets.Add(new IdentityServer4.Models.Secret(secret.Sha256()));
                    }
                }
                else
                {
                    //Keep existing secrets
                    foreach (var secret in api.Secrets)
                    {
                        isApi.ApiSecrets.Add(new IdentityServer4.Models.Secret(secret));
                    }
                }

            }

            if (api.Scopes != null)
            {
                foreach (var scope in api.Scopes)
                {
                    isApi.Scopes.Add(new IdentityServer4.Models.Scope(scope.Name, scope.DisplayName));
                }
            }

            var isEntApi = isApi.ToEntity();
            isEntApi.Id = api.dbid;

            return isEntApi;
        }

        internal object SaveApi(IdentityServer4.EntityFramework.Entities.ApiResource api)
        {
            bool result = false;

            try
            {
                var host = Program.host;
                using (var scope = host.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    using (var context = serviceProvider.GetRequiredService<ConfigurationDbContext>())
                    {
                        try
                        {
                            var dbApi = context.ApiResources
                                .Include(c => c.UserClaims)
                                .Include(c => c.Secrets)
                                .Include(c => c.Scopes)
                                .FirstOrDefault(x => x.Id == api.Id);

                            if (dbApi != null)
                            {
                                //UPDATE
                                dbApi.Name = api.Name;
                                dbApi.DisplayName = api.DisplayName;
                                dbApi.Description = api.Description;
                                dbApi.UserClaims = api.UserClaims;
                                dbApi.Secrets = api.Secrets;
                                dbApi.Scopes = api.Scopes;
                            }
                            else
                            {
                                //ADD
                                context.ApiResources.Add(api);
                            }

                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                            logger.LogError(ex, "Unabled to save ApiResources to DB.");

                            throw ex;
                        }
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        internal bool DeleteApi(int Id, out Exception error)
        {
            bool result = false;
            error = null;

            var host = Program.host;
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                using (var context = serviceProvider.GetRequiredService<ConfigurationDbContext>())
                {
                    try
                    {
                        var api = context.ApiResources
                            .Include(c => c.UserClaims)
                            .Include(c => c.Secrets)
                            .Include(c => c.Scopes)
                            .FirstOrDefault(x => x.Id == Id);

                        if (api != null)
                        {
                            context.ApiResources.Remove(api);
                            context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to delete ApiResource from DB.");

                        error = ex;
                    }

                }
            }

            return result;
        }

        internal List<Role> GetRoles(out Exception error)
        {
            var roles = new List<Role>();
            error = null;

            var host = Program.host;
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                using (var roleManager = serviceProvider.GetRequiredService<RoleManager<SAEONRole>>())
                {
                    try
                    {
                        roles = roleManager.Roles.Select(x => new Role() { Id = x.Id, Name = x.Name }).ToList();

                        
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to get Roles from DB.");

                        error = ex;
                    }
                }
            }

            return roles;
        }

        internal async Task<bool> AddUserRole(Guid userId, string roleName)
        {
            bool result = false;

            var host = Program.host;
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                using (var userManager = serviceProvider.GetRequiredService<UserManager<SAEONUser>>())
                {
                    try
                    {
                        var dbUser = userManager.Users.FirstOrDefault(x => x.Id == userId);
                        if(dbUser != null)
                        {
                            await userManager.AddToRoleAsync(dbUser, roleName);
                            result = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to add User to Role.");
                    }
                }
            }

            return result;
        }

        internal async Task<bool> RemoveUserRole(Guid userId, string roleName)
        {
            bool result = false;

            var host = Program.host;
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                using (var userManager = serviceProvider.GetRequiredService<UserManager<SAEONUser>>())
                {
                    try
                    {
                        var dbUser = userManager.Users.FirstOrDefault(x => x.Id == userId);
                        if (dbUser != null)
                        {
                            await userManager.RemoveFromRoleAsync(dbUser, roleName);
                            result = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to remove User from Role.");
                    }
                }
            }

            return result;
        }

        internal List<User> GetUserResources(out Exception error)
        {
            var userResources = new List<User>();
            error = null;

            var host = Program.host;
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                using (var userManager = serviceProvider.GetRequiredService<UserManager<SAEONUser>>())
                {
                    try
                    {
                        var dbUsers = userManager.Users
                            .OrderBy(x => (x.FirstName + " " + x.Surname))
                            .ToList();

                        foreach (var dbUser in dbUsers)
                        {
                            var dbRoles = userManager.GetRolesAsync(dbUser).Result.ToList();

                            userResources.Add(new User()
                            {
                                Id = dbUser.Id,
                                Name = dbUser.FirstName,
                                Surname = dbUser.Surname,
                                Email = dbUser.Email,
                                Password = dbUser.PasswordHash,
                                Roles = dbRoles
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to get User Resources from DB.");

                        error = ex;
                    }
                }
            }

            return userResources;
        }

        internal async Task<bool> SaveRolesAsync(List<Role> roles)
        {
            bool result = false;

            try
            {
                var host = Program.host;
                using (var scope = host.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    using (var roleManager = serviceProvider.GetRequiredService<RoleManager<SAEONRole>>())
                    {
                        try
                        {
                            foreach (var role in roles)
                            {
                                var identityRole = roleManager.Roles.FirstOrDefault(x => x.Id == role.Id);
                                if (identityRole != null)
                                {
                                    if (identityRole.Name != role.Name)
                                    {
                                        //UPDATE
                                        //Update Role Name
                                        identityRole.Name = role.Name;
                                        var identityResult = await roleManager.UpdateAsync(identityRole);
                                        if (!identityResult.Succeeded)
                                        {
                                            throw new Exception("Role Name updated failed");
                                        }

                                        //Remove Role Claim
                                        identityResult = await roleManager.RemoveClaimAsync(identityRole, 
                                            roleManager.GetClaimsAsync(identityRole).Result.FirstOrDefault(x => x.Type == ClaimTypes.Role));
                                        if (!identityResult.Succeeded)
                                        {
                                            throw new Exception("Add Role-Claim failed");
                                        }

                                        //Add new Role Claim
                                        identityResult = await roleManager.AddClaimAsync(identityRole, new Claim(ClaimTypes.Role, role.Name));
                                        if (!identityResult.Succeeded)
                                        {
                                            throw new Exception("Add Role-Claim failed");
                                        }
                                    }
                                }
                                else
                                {
                                    //ADD
                                    identityRole = new SAEONRole() { Id = role.Id, Name = role.Name };
                                    var identityResult = await roleManager.CreateAsync(identityRole);
                                    if(!identityResult.Succeeded)
                                    {
                                        throw new Exception("Create Role failed");
                                    }

                                    identityResult = await roleManager.AddClaimAsync(identityRole, new Claim(ClaimTypes.Role, role.Name));
                                    if (!identityResult.Succeeded)
                                    {
                                        throw new Exception("Add Role-Claim failed");
                                    }
                                }
                            }

                            foreach(var dbRole in roleManager.Roles)
                            {
                                if(!roles.Any(x => x.Id == dbRole.Id))
                                {
                                    //DELETE
                                    //Delete role
                                    var identityResult = await roleManager.DeleteAsync(dbRole);
                                    if (!identityResult.Succeeded)
                                    {
                                        throw new Exception("Delete Role failed");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                            logger.LogError(ex, "Unabled to save Roles to DB.");

                            throw ex;
                        }
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}
