using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SAEON.Identity.Service.UI
{
    public class ManageViewModel
    {
        public bool HasPassword { get; set; }
        public bool HasExternalLogins { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }

        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string DisplayNnme { get => $"{FirstName} {Surname} ({Email})"; }

        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public string StatusMessage { get; set; }
    }
}
