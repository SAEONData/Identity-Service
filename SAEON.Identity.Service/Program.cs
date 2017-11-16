using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using SAEON.Identity.Service.Data;
using SAEON.Logs;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.Mappers;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace SAEON.Identity.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                try
                {
                    // Requires using RazorPagesMovie.Models;
                    InitializeDbAsync(serviceProvider).Wait();
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
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("connectionStrings.json", optional: false, reloadOnChange: true);
                })
                .Build();

        private static async Task InitializeDbAsync(IServiceProvider serviceProvider)
        {
            async Task AddUserAsync(string firstName, string surname, string email, string password, string[] roles, UserManager<SAEONUser> userManager)
            {
                using (Logging.MethodCall(typeof(Program), new ParameterList { { "FirstName", firstName }, { "Surname", surname }, { "Email", email } }))
                {

                    if (await userManager.FindByNameAsync(email) == null)
                    {
                        await userManager.CreateAsync(new SAEONUser { UserName = email, Email = email, FirstName = firstName, Surname = surname, EmailConfirmed = true }, password);
                    }
                    var user = await userManager.FindByNameAsync(email);
                    if ((user != null) && (roles != null))
                        foreach (var role in roles)
                        {
                            if (!await userManager.IsInRoleAsync(user, role))
                            {
                                await userManager.AddToRoleAsync(user, role);
                            }
                        }
                }
            }

            using (Logging.MethodCall(typeof(Program)))
            {
                serviceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
                serviceProvider.GetRequiredService<SAEONDbContext>().Database.Migrate();

                var context = serviceProvider.GetRequiredService<ConfigurationDbContext>();

                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                var roleManager = serviceProvider.GetRequiredService<RoleManager<SAEONRole>>();
                string[] roles = new string[] { "SAEON.Admin", "SAEON.Identity.Service", "SAEON.Observations.Admin" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var identityRole = new SAEONRole { Name = role };
                        await roleManager.CreateAsync(identityRole);
                        await roleManager.AddClaimAsync(identityRole, new Claim(ClaimTypes.Role, role));
                    }
                }
                var userManager = serviceProvider.GetRequiredService<UserManager<SAEONUser>>();
                await AddUserAsync("Administrator", "SAEON", "admin@saeon.ac.za", "0d3DHCClCsAh", roles, userManager);
                await AddUserAsync("Tim", "Parker-Nance", "timpn@saeon.ac.za", "T1mS@E0N", roles, userManager);
                //await AddUserAsync("Mike", "Metcalfe", "mike@webtide.co.za", "M1keWebT1de", null, userManager);
                //await AddUserAsync("Lunga", "WebTide", "lunga@webtide.co.za", "Lung@WebT1de", null, userManager);
                //await AddUserAsync("Guest", "SAEON", "guest@saeon.ac.za", "S@E0NGue$t", new string[] { "Observations.Reader" }, userManager);
                //await AddUserAsync("Admin", "SAEON", "admin@saeon.ac.za", "S@E0N@dm1n", new string[] { "Observations.Admin" }, userManager);
            }
        }
    }

}
