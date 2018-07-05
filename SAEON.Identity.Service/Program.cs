using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAEON.Identity.Service.Config;
using Serilog;
using System;

namespace SAEON.Identity.Service
{
    public class Program
    {
        public static IWebHost host;

        public static void Main(string[] args)
        {
            host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                try
                {
                    // Requires using RazorPagesMovie.Models;
                    Settings.InitializeDbAsync(serviceProvider).Wait();
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost
                .CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("connectionStrings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile("Config/Settings.json", optional: false, reloadOnChange: true);
                })
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();

    }

}
