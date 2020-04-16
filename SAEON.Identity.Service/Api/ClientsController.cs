using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAEON.Logs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public ClientsController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpPost]
        public async Task<IActionResult> AddNodesClientAsync()
        {
            using (Logging.MethodCall(GetType()))
            {
                try
                {
                    var clientNode = new Client
                    {
                        ClientId = "SAEON.Observations.WebAPI.Nodes",
                        ClientName = "Client for WebAPI use by SAEON Nodes",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        AccessTokenLifetime = (int)TimeSpan.FromDays(1).TotalSeconds,
                        ClientSecrets = new List<Secret> { new Secret("6dU7cp1#n4qS".Sha256()) },
                        AllowedScopes = new List<string> { "openid", "email", "SAEON.Observations.WebAPI" }
                    };
                    var clientSAEIS = new Client
                    {
                        ClientId = "SAEON.Observations.WebAPI.SAEIS",
                        ClientName = "Client for WebAPI use by the South African Estuary Information System",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        AccessTokenLifetime = (int)TimeSpan.FromDays(1).TotalSeconds,
                        ClientSecrets = new List<Secret> { new Secret("2AD6Dmf4$tNi".Sha256()) },
                        AllowedScopes = new List<string> { "openid", "email", "SAEON.Observations.WebAPI" }
                    };

                    var context = serviceProvider.GetRequiredService<ConfigurationDbContext>();
                    if (!await context.Clients.AnyAsync(i => i.ClientName == clientNode.ClientName))
                    {
                        await context.Clients.AddAsync(clientNode.ToEntity());
                        context.SaveChanges();
                    }
                    if (!await context.Clients.AnyAsync(i => i.ClientName == clientSAEIS.ClientName))
                    {
                        await context.Clients.AddAsync(clientSAEIS.ToEntity());
                        context.SaveChanges();
                    }
                    return Ok("Client added");
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex);
                    throw;
                }
            }
        }
    }
}