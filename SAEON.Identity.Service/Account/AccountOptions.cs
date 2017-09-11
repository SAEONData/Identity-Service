using System;

namespace SAEON.Identity.Service.UI
{
    public class AccountOptions
    {
        public static bool AllowLocalLogin = true;
        public static bool AllowRememberLogin = true;
        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

        public static bool ShowLogoutPrompt = true;
        public static bool AutomaticRedirectAfterSignOut = true;

        // to enable windows authentication, the host (IIS or IIS Express) also must have 
        // windows auth enabled.
        public static bool WindowsAuthenticationEnabled = false;
        public static bool IncludeWindowsGroups = false;
        // specify the Windows authentication schemes you want to use for authentication
        public static readonly string[] WindowsAuthenticationSchemes = new string[] { "Negotiate", "NTLM" };
        public static readonly string WindowsAuthenticationProviderName = "Windows";
        public static readonly string WindowsAuthenticationDisplayName = "Windows";

        public static string InvalidCredentialsErrorMessage = "Invalid username or password";
        public static string AlreadyRegisteredErrorMessage = "User already registered";
    }
}
