using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        public ActionResult ApiResources()
        {
            return View();
        }


    }

    public class ConfigControllerLogic
    {
        public IdentityServer4.EntityFramework.Entities.Client GetClientResource(string clientId)
        {
            IdentityServer4.EntityFramework.Entities.Client clientResource = new IdentityServer4.EntityFramework.Entities.Client();

            var tmpClientResource = GetClientResources().FirstOrDefault(x => x.ClientId == clientId);
            if (tmpClientResource != null)
            {
                clientResource = tmpClientResource;
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
                        clientResources = context.Clients.OrderBy(c => c.ClientName).ToList();
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Unabled to get ClientResource from DB.");
                    }

                }
            }

            return clientResources;
        }
    }
}