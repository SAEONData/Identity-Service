using Microsoft.AspNetCore.Hosting.Internal;
using SAEON.Logs;
using System;
using System.Security.Cryptography.X509Certificates;

namespace SAEON.Identity.Service.Config
{
    internal static class Cert
    {
        public static X509Certificate2 Load()
        {
            using (Logging.MethodCall(typeof(Cert)))
                try
                {
                    //Logging.Information("RootFolder: {RootFolder}", rootFolder);
                    //var fileName = rootFolder + "/bin/config/saeon.ac.za.pfx";
                    var fileName = "Config/saeon.ac.za.pfx";
                    Logging.Information($"Loading {fileName}");
                    return new X509Certificate2(fileName, "S@E0N.Cert");
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex, "Unable to load certificate");
                    throw;
                }
        }

    }
}
