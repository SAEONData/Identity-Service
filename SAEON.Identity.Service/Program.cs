using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAEON.Identity.Service.Data;
using SAEON.Logs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    Configuration.InitializeDbAsync(serviceProvider).GetAwaiter().GetResult();
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
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("secrets.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile("api\\secrets.json", optional: true, reloadOnChange: true);
                })
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();

    }

    internal static class Configuration
    {
        internal static async Task InitializeDbAsync(IServiceProvider serviceProvider)
        {
            using (Logging.MethodCall(typeof(Program)))
            {
                Logging.Information("Seeding database");
                serviceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
                serviceProvider.GetRequiredService<SAEONDbContext>().Database.Migrate();
                var context = serviceProvider.GetRequiredService<ConfigurationDbContext>();
                var config = serviceProvider.GetRequiredService<IConfiguration>();

                foreach (var resource in GetIdentityResources())
                {
                    if (!await context.IdentityResources.AnyAsync(i => i.Name == resource.Name))
                    {
                        await context.IdentityResources.AddAsync(resource.ToEntity());
                    }
                }
                context.SaveChanges();
            }
        }

        internal static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource> {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email { Required=true},
                new IdentityResource {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };
        }
    }

}
