using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Config
{
    public class UserAvailableRolesViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Guid userId)
        {          
            return await Task.Run(() => {

                var _logic = new ConfigControllerLogic();

                var roles = _logic.GetRoles();
                var user = _logic.GetUserResources().FirstOrDefault(x => x.Id == userId);
                var availableRoles = roles.Where(x => !user.Roles.Contains(x.Name)).ToList();

                return View("AvailableRolesPartial", availableRoles);
            });
        }
    }
}
