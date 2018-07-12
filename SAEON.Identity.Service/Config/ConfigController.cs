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
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using SAEON.Identity.Service.Helpers;

namespace SAEON.Identity.Service.Config
{
    [Authorize(Roles = "SAEON.Admin")]
    public class ConfigController : Controller
    {
        ConfigControllerLogic _logic;

        public ConfigController()
        {
            _logic = new ConfigControllerLogic();
        }

        public ActionResult ClientResources()
        {
            var data = _logic.GetClientResources(out Exception error);
            if(error != null)
            {
                HelperFunctions.AddErrors(error, ModelState);
            }

            return View(data);
        }

        public ActionResult ClientResourceAdd()
        {
            var data = _logic.GetClientResource(0, out Exception error);
            if(error != null)
            {
                HelperFunctions.AddErrors(error, ModelState);
            }

            return View("ClientResourceEdit", data);
        }

        public ActionResult ClientResourceEdit(int Id)
        {
            var data = _logic.GetClientResource(Id, out Exception error);
            if (error != null)
            {
                HelperFunctions.AddErrors(error, ModelState);
            }

            return View(data);
        }

        public ActionResult ClientResourceDelete(int Id)
        {
            bool result = _logic.DeleteClient(Id, out Exception error);
            return RedirectToAction("ClientResources");
        }

        [HttpPost]
        public ActionResult ClientResourceAdd(Client client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Convert back to IS Client
                    var isClient = _logic.BuildClient(client);

                    //Save changes to DB
                    var result = _logic.SaveClient(isClient);

                    return RedirectToAction("ClientResources");
                }
                catch (Exception ex)
                {
                    HelperFunctions.AddErrors(ex, ModelState);
                }
            }

            return View(client);
        }

        [HttpPost]
        public ActionResult ClientResourceEdit(Client client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Convert back to IS Client
                    var isClient = _logic.BuildClient(client);

                    //Save changes to DB
                    var result = _logic.SaveClient(isClient);

                    return RedirectToAction("ClientResources");
                }
                catch (Exception ex)
                {
                    HelperFunctions.AddErrors(ex, ModelState);
                }
            }

            return View(client);
        }

        public ActionResult ApiResources()
        {
            var data = _logic.GetApiResources(out Exception error);
            if(error != null)
            {
                HelperFunctions.AddErrors(error, ModelState);
            }

            return View(data);
        }

        public ActionResult ApiResourceEdit(int Id)
        {
            var data = _logic.GetApiResource(Id, out Exception error);
            if(error != null)
            {
                HelperFunctions.AddErrors(error, ModelState);
            }

            return View(data);
        }

        public ActionResult ApiResourceAdd()
        {
            var data = _logic.GetApiResource(0, out Exception error);
            if(error != null)
            {
                HelperFunctions.AddErrors(error, ModelState);
            }

            return View("ApiResourceEdit", data);
        }

        [HttpPost]
        public ActionResult ApiResourceEdit(API api)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Convert back to IS Client
                    var isApi = _logic.BuildApi(api);

                    //Save changes to DB
                    var result = _logic.SaveApi(isApi);

                    return RedirectToAction("ApiResources");
                }
                catch (Exception ex)
                {
                    HelperFunctions.AddErrors(ex, ModelState);
                }
            }

            return View(api);
        }

        [HttpPost]
        public ActionResult ApiResourceAdd(API api)
        {
            if (ModelState.IsValid)
            {
                //Convert back to IS Client
                try
                {
                    var isApi = _logic.BuildApi(api);

                    //Save changes to DB
                    var result = _logic.SaveApi(isApi);

                    return RedirectToAction("ApiResources");
                }
                catch (Exception ex)
                {
                    HelperFunctions.AddErrors(ex, ModelState);
                }
            }

            return View(api);
        }

        public ActionResult ApiResourceDelete(int Id)
        {
            bool result = _logic.DeleteApi(Id, out Exception error);
            if(error != null)
            {
                HelperFunctions.AddErrors(error, ModelState);
            }

            return RedirectToAction("ApiResources");
        }

        public ActionResult UserRoleManagement()
        {
            return View();
        }

        public ActionResult ManageUserRoles()
        {
            var data = _logic.GetRoles(out Exception error);
            if(error != null)
            {
                HelperFunctions.AddErrors(error, ModelState);
            }

            return View(data);
        }

        [HttpPost]
        public ActionResult UpdateRoles([FromBody] List<Role> roles)
        {
            return PartialView("RolesPartial", roles);
        }

        [HttpPost]
        public ActionResult ManageUserRoles(List<Role> roles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Save changes to DB
                    var result = _logic.SaveRolesAsync(roles).Result;

                    return RedirectToAction("UserRoleManagement");
                }
                catch (Exception ex)
                {
                    HelperFunctions.AddErrors(ex, ModelState);
                }
            }

            return View(roles);
        }

        public ActionResult AssignUserRoles()
        {
            var data = _logic.GetUserResources(out Exception error);
            if(error != null)
            {
                HelperFunctions.AddErrors(error, ModelState);
            }

            return View(data);
        }

        [HttpPost]
        public IActionResult AvailableRolesPartial([FromBody] JObject data)
        {
            var userId = data["userId"].ToObject<Guid>();
            var assignRole = data["assignRole"] != null ? data["assignRole"].ToObject<string>() : "";
            return ViewComponent("UserAvailableRoles", new { userId, assignRole });
        }

        [HttpPost]
        public IActionResult AssignedRolesPartial([FromBody] JObject data)
        {
            var userId = data["userId"].ToObject<Guid>();
            var removeRole = data["removeRole"] != null ? data["removeRole"].ToObject<string>() : "";
            return ViewComponent("UserAssignedRoles", new { userId, removeRole });
        }
    }

}