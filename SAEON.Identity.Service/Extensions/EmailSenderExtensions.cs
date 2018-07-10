using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.UI
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>." +
                $"<br/><br/>" +
                $"Alternatively you can copy this URL into your web browser:<br/>" +
                $"{link}");
        }
    }
}
