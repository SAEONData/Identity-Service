using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SAEON.Identity.Service.Data;
using SAEON.Identity.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Config
{
    public class UserAvailableRolesViewComponent : ViewComponent
    {
        private readonly ConfigurationDbContext dbContext;
        private readonly UserManager<SAEONUser> userManager;
        private readonly RoleManager<SAEONRole> roleManager;

        public UserAvailableRolesViewComponent(ConfigurationDbContext dbContext, UserManager<SAEONUser> userManager, RoleManager<SAEONRole> roleManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid userId, string assignRole)
        {
            return await Task.Run(() =>
            {

                var availableRoles = new List<Role>();
                var _logic = new ConfigControllerLogic(dbContext, userManager, roleManager);

                if (!string.IsNullOrEmpty(assignRole))
                {
                    var result = _logic.AddUserRole(userId, assignRole).Result;
                }

                var roles = _logic.GetRoles(out Exception error);
                if (error != null)
                {
                    HelperFunctions.AddErrors(error, ModelState);
                }
                else
                {
                    var data = _logic.GetUserResources(out error);
                    if (error != null)
                    {
                        HelperFunctions.AddErrors(error, ModelState);
                    }
                    else
                    {
                        var user = data.FirstOrDefault(x => x.Id == userId);
                        availableRoles = roles.Where(x => !user.Roles.Contains(x.Name)).OrderBy(x => x.Name).ToList();
                    }
                }

                return View("AvailableRolesPartial", availableRoles);
            });
        }
    }
}
