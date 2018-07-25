using Microsoft.AspNetCore.Mvc;
using SAEON.Identity.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Config
{
    public class UserAssignedRolesViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Guid userId, string removeRole)
        {
            return await Task.Run(() => {

                var assignedRoles = new List<Role>();
                var _logic = new ConfigControllerLogic();

                if (!string.IsNullOrEmpty(removeRole))
                {
                    var result = _logic.RemoveUserRole(userId, removeRole).Result;
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
                        assignedRoles = roles.Where(x => user.Roles.Contains(x.Name)).OrderBy(x => x.Name).ToList();
                    }
                }

                return View("AssignedRolesPartial", assignedRoles);
            });
        }
    }
}
