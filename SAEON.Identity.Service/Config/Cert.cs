using SAEON.Logs;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SAEON.Identity.Service.Config
{
    internal static class Cert
    {
        public static X509Certificate2 Load()
        {
            using (SAEONLogs.MethodCall(typeof(Cert)))
                try
                {
                    //SAEONLogs.Information("RootFolder: {RootFolder}", rootFolder);
                    //var fileName = rootFolder + "/bin/config/saeon.ac.za.pfx";
                    var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Config\\saeon.ac.za.pfx");
                    SAEONLogs.Information($"Loading {fileName}");
                    return new X509Certificate2(fileName, "S@E0N.Cert");
                }
                catch (Exception ex)
                {
                    SAEONLogs.Exception(ex, "Unable to load certificate");
                    throw;
                }
        }

    }
}
