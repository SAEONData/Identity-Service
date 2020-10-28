using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SAEON.Logs;
using System;

namespace SAEON.Identity.Service
{
    public class Program
    {
        public static IWebHost host;

        public static void Main(string[] args)
        {
            SAEONLogs.CreateConfiguration().Initialize();
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                SAEONLogs.Exception(ex);
                throw;
            }
            finally
            {
                SAEONLogs.ShutDown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSAEONLogs()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureAppConfiguration((hostContext, config) =>
                        {
                            config.AddJsonFile("secrets.json", optional: false, reloadOnChange: true);
                            config.AddJsonFile("api\\secrets.json", optional: true, reloadOnChange: true);
                        });
                    webBuilder.UseStartup<Startup>();
                });
    }

}
