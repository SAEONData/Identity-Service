using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SAEON.Logs;
using System;
using System.Collections.Generic;

namespace SAEON.Identity.Service.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration config;

        public ClientsController(IServiceProvider serviceProvider, IConfiguration config)
        {
            this.serviceProvider = serviceProvider;
            this.config = config;
        }

        [HttpPost]
        //public async Task<IActionResult> AddNodesClientAsync()
        public IActionResult AddNodesClient()
        {
            using (SAEONLogs.MethodCall(GetType()))
            {
                try
                {
                    /*
                    var serviceConfiguration = config.GetSection("IdentityService");
                    var configClients = new List<ConfigClient>();
                    serviceConfiguration.GetSection("Clients").Bind(configClients);
                    SAEONLogs.Information("Clients: {@clients}", configClients);
                    var context = serviceProvider.GetRequiredService<ConfigurationDbContext>();
                    foreach (var configClient in configClients)
                    {
                        var client = new IdentityServer4.Models.Client
                        {
                            ClientId = configClient.Id,
                            ClientName = configClient.Name,
                            RequireConsent = configClient.RequireConsent,
                            AllowRememberConsent = configClient.RememberConsent,
                            AllowOfflineAccess = configClient.OfflineAccess,
                            AllowAccessTokensViaBrowser = configClient.AccessTokensViaBrowser,
                            IdentityTokenLifetime = configClient.IdentityTokenLifetime,
                            AccessTokenLifetime = configClient.AccessTokenLifetime
                        };
                        switch (configClient.GrantType)
                        {
                            case "ClientCredentials":
                                client.AllowedGrantTypes = GrantTypes.ClientCredentials;
                                break;
                            case "Code":
                                client.AllowedGrantTypes = GrantTypes.Code;
                                break;
                            case "CodeAndClientCredentials":
                                client.AllowedGrantTypes = GrantTypes.CodeAndClientCredentials;
                                break;
                            case "Hybrid":
                                client.AllowedGrantTypes = GrantTypes.Hybrid;
                                break;
                            case "HybridAndClientCredentials":
                                client.AllowedGrantTypes = GrantTypes.HybridAndClientCredentials;
                                break;
                            case "Implicit":
                                client.AllowedGrantTypes = GrantTypes.Implicit;
                                break;
                            case "ImplicitAndClientCredentials":
                                client.AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials;
                                break;
                            case "ResourceOwnerPassword":
                                client.AllowedGrantTypes = GrantTypes.ResourceOwnerPassword;
                                break;
                            case "ResourceOwnerPasswordAndClientCredentials":
                                client.AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials;
                                break;
                            default:
                                SAEONLogs.Error("Unknown GrantType: {GranType}", configClient.GrantType);
                                continue;
                        }
                        foreach (var secret in configClient.Secrets)
                        {
                            client.ClientSecrets.Add(new Secret(secret.Sha256()));
                        }
                        foreach (var scope in configClient.Scopes)
                        {
                            client.AllowedScopes.Add(scope);
                        }
                        foreach (var corsOrigin in configClient.CorsOrigins)
                        {
                            client.AllowedCorsOrigins.Add(corsOrigin);
                        }
                        foreach (var uri in configClient.RedirectURIs)
                        {
                            client.RedirectUris.Add(uri);
                        }
                        foreach (var uri in configClient.PostLogoutRedirectURIs)
                        {
                            client.PostLogoutRedirectUris.Add(uri);
                        }
                        SAEONLogs.Information("Client: {@client}", client);
                        var isclients = await context.Clients.Select(i => i.ClientId).ToListAsync();
                        SAEONLogs.Information("ClientIds: {ClientIds}", isclients);
                        if (!await context.Clients.AnyAsync(i => i.ClientId == client.ClientId))
                        {
                            await context.Clients.AddAsync(client.ToEntity());
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    var isClient = await context.Clients.Include(i => i.ClientSecrets).FirstAsync(i => i.ClientId == client.ClientId);
                        //    SAEONLogs.Information("Client: {@Client}", isClient);
                        //    isClient.ClientName = client.ClientName;
                        //    isClient.ClientSecrets.Clear();
                        //    foreach (var secret in client.ClientSecrets)
                        //    {
                        //        var clientSecret = new IdentityServer4.EntityFramework.Entities.ClientSecret
                        //        {
                        //            ClientId = isClient.Id,
                        //            Type = secret.Type,
                        //            Value = secret.Value
                        //        };
                        //        isClient.ClientSecrets.Add(clientSecret);
                        //    }
                        //    context.Update(isClient);
                        //    context.SaveChanges();
                        //}
                    }
                    */
                    return Ok("Client(s) added");
                }
                catch (Exception ex)
                {
                    SAEONLogs.Exception(ex);
                    throw;
                }
            }
        }
    }

    public class ConfigClient
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

}