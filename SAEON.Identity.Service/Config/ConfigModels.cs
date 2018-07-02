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
        public int dbid { get; set; } = 0;

        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string GrantType { get; set; } = "";
        public bool RequireConsent { get; set; } = false;
        public bool RememberConsent { get; set; } = false;
        public bool OfflineAccess { get; set; } = false;
        public bool AccessTokensViaBrowser { get; set; } = false;
        public int IdentityTokenLifetime { get; set; } = (int)new TimeSpan(14, 0, 0).TotalSeconds;
        public int AccessTokenLifetime { get; set; } = (int)new TimeSpan(7, 0, 0).TotalSeconds;
        public List<string> Secrets { get; set; } = new List<string>();
        public List<string> Scopes { get; set; } = new List<string>();
        public List<string> CorsOrigins { get; set; } = new List<string>();
        public List<string> RedirectURIs { get; set; } = new List<string>();
        public List<string> PostLogoutRedirectURIs { get; set; } = new List<string>();

        public string Secrets_Combined
        {
            get => value_combine(Secrets);
            set => Secrets = value_split(value);
        }

        public string Scopes_Combined
        {
            get => value_combine(Scopes);
            set => Scopes = value_split(value);
        }

        public string CorsOrigins_Combined
        {
            get => value_combine(CorsOrigins);
            set => CorsOrigins = value_split(value);
        }

        public string RedirectURIs_Combined
        {
            get => value_combine(RedirectURIs);
            set => RedirectURIs = value_split(value);
        }

        public string PostLogoutRedirectURIs_Combined
        {
            get => value_combine(PostLogoutRedirectURIs);
            set => PostLogoutRedirectURIs = value_split(value);
        }


        private string value_combine(List<string> values)
        {
            if (values != null && values.Count > 0)
            {
                return string.Join(Environment.NewLine, values);
            }
            else
            {
                return "";
            }
        }

        private List<string> value_split(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Split(Environment.NewLine).ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
