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

        public ActionResult ClientResourceDelete(string clientId)
        {
            bool result = _logic.DeleteClient(clientId);

            return RedirectToAction("ClientResources");
        }

        [HttpPost]
        public ActionResult ClientResourceAdd(Client client)
        {
            if (ModelState.IsValid)
            {
                //Convert back to IS Client
                var isClient = _logic.BuildClient(client);

                //Save changes to DB
                var result = _logic.SaveClient(isClient);

                return RedirectToAction("ClientResources");
            }

            return View(client);
        }

        [HttpPost]
        public ActionResult ClientResourceEdit(Client client)
        {
            if (ModelState.IsValid)
            {
                //Convert back to IS Client
                var isClient = _logic.BuildClient(client);

                //Save changes to DB
                var result = _logic.SaveClient(isClient);

                return RedirectToAction("ClientResources");
            }

            return View(client);
        }

        public ActionResult ApiResources()
        {
            return View();
        }
    }

}