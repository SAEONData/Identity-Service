using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Helpers
{
    public static class HelperFunctions
    {
        #region Helpers

        public static void AddErrors(Exception ex, ModelStateDictionary ModelState)
        {
            foreach (var errMsg in GetExceptionMessages(ex))
            {
                ModelState.AddModelError(string.Empty, errMsg);
            }
        }

        public static List<string> GetExceptionMessages(Exception ex)
        {
            var messages = new List<string>();
            messages.Add(ex.Message);

            if (ex.InnerException != null)
            {
                messages.AddRange(GetExceptionMessages(ex.InnerException));
            }

            return messages;
        }

        #endregion
    }
}
