using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SAEON.Identity.Service.Config
{
    public class ConfigController : Controller
    {
        ConfigControllerLogic _logic;

        public ConfigController()
        {
            _logic = new ConfigControllerLogic();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ClientResources()
        {
            return View(_logic.GetClientResources());
        }

        public ActionResult ClientResourceAdd()
        {
            return View("ClientResourceEdit", _logic.GetClientResource(""));
        }

        public ActionResult ClientResourceEdit(string clientId)
        {
            return View(_logic.GetClientResource(clientId));
        }

        [HttpPost]
        public ActionResult ClientResourceAdd(IdentityServer4.EntityFramework.Entities.Client client)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save changes
                return RedirectToAction("Index");
            }

            return View(client);
        }

        [HttpPost]
        public ActionResult ClientResourceEdit(ClientViewModel client)
        {
            if (ModelState.IsValid)
            {
                // TODO:
                //Convert back to IS Client
                //Save changes to DB

                return RedirectToAction("ClientResources");
            }

            return View(client);
        }

        public ActionResult ApiResources()
        {
            return View();
        }
    }

    public class ClientViewModel
    {
        [Required]
        public string ClientId { get; set; } = "";
        [Required]
        public string ClientName { get; set; } = "";
        public string IdentityTokenLifetime { get; set; } = "";
        public string AccessTokenLifetime { get; set; } = "";
        public string AllowedGrantTypes { get; set; } = "";
        public string ClientSecrets { get; set; } = "";
        public string AllowedScopes { get; set; } = "";
        public string AllowedCorsOrigins { get; set; } = "";
        public string RedirectUris { get; set; } = "";
        public string PostLogoutRedirectUris { get; set; } = "";
        public bool RequireConsent { get; set; } = false;
        public bool AllowRememberConsent { get; set; } = false;
        public bool AllowOfflineAccess { get; set; } = false;
        public bool AllowAccessTokensViaBrowser { get; set; } = false;
    }

    public class ConfigControllerLogic
    {
        public ClientViewModel GetClientResource(string clientId)
        {
            ClientViewModel clientResource = new ClientViewModel();

            var data = GetClientResources().FirstOrDefault(x => x.ClientId == clientId);
            if (data != null)
            {
                clientResource = new ClientViewModel()
                {
                    ClientId = data.ClientId,
                    ClientName = data.ClientName,
                    IdentityTokenLifetime = data.IdentityTokenLifetime.ToString(),
                    AccessTokenLifetime = data.AccessTokenLifetime.ToString(),
                    AllowedGrantTypes = string.Join(Environment.NewLine, data.AllowedGrantTypes.Select(x => x.GrantType)),
                    ClientSecrets = string.Join(Environment.NewLine, data.ClientSecrets.Select(x => x.Value)),
                    AllowedScopes = string.Join(Environment.NewLine, data.AllowedScopes.Select(x => x.Scope)),
                    AllowedCorsOrigins = string.Join(Environment.NewLine, data.AllowedCorsOrigins.Select(x => x.Origin)),
                    RedirectUris = string.Join(Environment.NewLine, data.RedirectUris.Select(x => x.RedirectUri)),
                    PostLogoutRedirectUris = string.Join(Environment.NewLine, data.PostLogoutRedirectUris.Select(x => x.PostLogoutRedirectUri)),
                    RequireConsent = data.RequireConsent,
                    AllowRememberConsent = data.AllowRememberConsent,
                    AllowOfflineAccess = data.AllowOfflineAccess,
                    AllowAccessTokensViaBrowser = data.AllowAccessTokensViaBrowser
                };
            }

            return clientResource;
        }

        public List<IdentityServer4.EntityFramework.Entities.Client> GetClientResources()
        {
            var clientResources = new List<IdentityServer4.EntityFramework.Entities.Client>();

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
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to get ClientResources from DB.");
                    }

                }
            }

            return clientResources;
        }
    }
}