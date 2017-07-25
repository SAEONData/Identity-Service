using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.UI
{
    public class ManageViewModel : ManageInputModel
    {
        public bool HasPassword { get; set; }
        public bool HasExternalLogins { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
    }
}
