using Microsoft.AspNetCore.Mvc;
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

                var _logic = new ConfigControllerLogic();

                if (!string.IsNullOrEmpty(removeRole))
                {
                    var result = _logic.RemoveUserRole(userId, removeRole).Result;
                }

                var roles = _logic.GetRoles();
                var user = _logic.GetUserResources().FirstOrDefault(x => x.Id == userId);
                var assignedRoles = roles.Where(x => user.Roles.Contains(x.Name)).OrderBy(x => x.Name).ToList();

                return View("AssignedRolesPartial", assignedRoles);
            });
        }
    }
}
