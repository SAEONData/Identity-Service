using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Config
{
    public class User
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class Scope
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

    public class API
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<string> Claims { get; set; } = new List<string>();
        public List<string> Secrets { get; set; } = new List<string>();
        public List<Scope> Scopes { get; set; } = new List<Scope>();
    }

    public class Client
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GrantType { get; set; }
        public bool RequireConsent { get; set; }
        public bool RememberConsent { get; set; }
        public bool OfflineAccess { get; set; }
        public bool AccessTokensViaBrowser { get; set; }
        public int IdentityTokenLifetime { get; set; } = (int)new TimeSpan(14, 0, 0).TotalSeconds;
        public int AccessTokenLifetime { get; set; } = (int)new TimeSpan(7, 0, 0).TotalSeconds;
        public List<string> Secrets { get; set; } = new List<string>();
        public List<string> Scopes { get; set; } = new List<string>();
        public List<string> CorsOrigins { get; set; } = new List<string>();
        public List<string> RedirectURIs { get; set; } = new List<string>();
        public List<string> PostLogoutRedirectURIs { get; set; } = new List<string>();
    }
}
