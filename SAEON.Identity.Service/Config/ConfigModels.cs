using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Config
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = "";
    }

    public class User
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string PasswordReset { get; set; } = "";
        public string PasswordResetConfirm { get; set; } = "";
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> AvailableRoles { get; set; } = new List<string>();

        public string Roles_Combined
        {
            get => Utils.value_combine(Roles);
            set => Roles = Utils.value_split(value);
        }
    }

    public class Scope
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
    }

    public class API
    {
        public int dbid { get; set; } = 0;

        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Claims { get; set; } = new List<string>();
        public List<string> Secrets { get; set; } = new List<string>();
        public List<string> NewSecrets { get; set; } = new List<string>();
        public List<Scope> Scopes { get; set; } = new List<Scope>();

        public string Claims_Combined
        {
            get => Utils.value_combine(Claims);
            set => Claims = Utils.value_split(value);
        }

        public string Secrets_Combined
        {
            get => Utils.value_combine(Secrets);
            set => Secrets = Utils.value_split(value);
        }

        public string NewSecrets_Combined
        {
            get => Utils.value_combine(NewSecrets);
            set => NewSecrets = Utils.value_split(value);
        }

        public string Scopes_Combined
        {
            get => Utils.value_combine(Scopes.Select(x => x.Name).ToList());
            set => Scopes = Utils.value_split(value).Select(x => new Scope() { Name = x, DisplayName = x.Replace("_", " ") }).ToList();
        }
    }

    public class Client
    {
        public int dbid { get; set; } = 0;

        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string GrantType { get; set; } = "";
        public bool RequireConsent { get; set; } = false;
        public bool RememberConsent { get; set; } = false;
        public bool OfflineAccess { get; set; } = false;
        public bool AccessTokensViaBrowser { get; set; } = false;
        public int IdentityTokenLifetime { get; set; } = (int)new TimeSpan(14, 0, 0).TotalSeconds;
        public int AccessTokenLifetime { get; set; } = (int)new TimeSpan(7, 0, 0).TotalSeconds;
        public List<string> Secrets { get; set; } = new List<string>();
        public List<string> NewSecrets { get; set; } = new List<string>();
        public List<string> Scopes { get; set; } = new List<string>();
        public List<string> CorsOrigins { get; set; } = new List<string>();
        public List<string> RedirectURIs { get; set; } = new List<string>();
        public List<string> PostLogoutRedirectURIs { get; set; } = new List<string>();

        public string Secrets_Combined
        {
            get => Utils.value_combine(Secrets);
            set => Secrets = Utils.value_split(value);
        }

        public string NewSecrets_Combined
        {
            get => Utils.value_combine(NewSecrets);
            set => NewSecrets = Utils.value_split(value);
        }

        public string Scopes_Combined
        {
            get => Utils.value_combine(Scopes);
            set => Scopes = Utils.value_split(value);
        }

        public string CorsOrigins_Combined
        {
            get => Utils.value_combine(CorsOrigins);
            set => CorsOrigins = Utils.value_split(value);
        }

        public string RedirectURIs_Combined
        {
            get => Utils.value_combine(RedirectURIs);
            set => RedirectURIs = Utils.value_split(value);
        }

        public string PostLogoutRedirectURIs_Combined
        {
            get => Utils.value_combine(PostLogoutRedirectURIs);
            set => PostLogoutRedirectURIs = Utils.value_split(value);
        }



    }

    internal static class Utils
    {
        internal static string value_combine(List<string> values)
        {
            if (values != null && values.Count > 0)
            {
                return string.Join(Environment.NewLine, values.Where(x => !string.IsNullOrEmpty(x)));
            }
            else
            {
                return "";
            }
        }

        internal static List<string> value_split(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
