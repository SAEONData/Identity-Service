using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAEON.Identity.Service.Data;
using SAEON.Identity.Service.Services;
using SAEON.Logs;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SAEON.Identity.Service
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("connectionStrings.json", optional: false, reloadOnChange: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            Logging
                .CreateConfiguration("Logs/SAEON.Identity.Service {Date}.txt", Configuration)
                .Create();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (Logging.MethodCall(GetType()))
            {
                var connectionString = Configuration.GetConnectionString("IdentityService");
                var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
                services.AddDbContext<SAEONDbContext>(options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationsAssembly).EnableRetryOnFailure()));

                services
                    .AddIdentity<SAEONUser, SAEONRole>(config =>
                    {
                        config.User.RequireUniqueEmail = true;
                        config.Lockout = new LockoutOptions
                        {
                            AllowedForNewUsers = true,
                            DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30),
                            MaxFailedAccessAttempts = 5
                        };
                        config.Password = new PasswordOptions
                        {
                            RequireDigit = true,
                            RequireNonAlphanumeric = false,
                            RequireUppercase = true,
                            RequireLowercase = true,
                            RequiredLength = 8,
                        };
                    })
                    .AddEntityFrameworkStores<SAEONDbContext, Guid>()
                    .AddDefaultTokenProviders();
                services
                    .AddIdentityServer(config =>
                    {
                        config.Events = new EventsOptions
                        {
                            RaiseErrorEvents = true,
                            RaiseFailureEvents = true,
                            RaiseInformationEvents = true,
                            RaiseSuccessEvents = true
                        };
                    })
                    .AddOperationalStore(builder => builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly).EnableRetryOnFailure()))
                    .AddConfigurationStore(builder => builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly).EnableRetryOnFailure()))
                    .AddAspNetIdentity<SAEONUser>()
                    .AddTemporarySigningCredential();

                services.AddMvc(options =>
                {
                    options.Filters.Add(typeof(SecurityHeadersAttribute));
                });
                services.AddLogging();

                services.AddSingleton<IConfiguration>(Configuration);
                // Add application services.
                services.AddTransient<IEmailSender, AuthMessageSender>();
                services.AddTransient<ISmsSender, AuthMessageSender>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            using (Logging.MethodCall(GetType()))
            {
                loggerFactory
                    .AddConsole(Configuration.GetSection("Logging"))
                    .AddDebug()
                    .AddSerilog();

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseDatabaseErrorPage();
                    app.UseBrowserLink();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                }
                Logging.Information("Environment: {environment}", env.EnvironmentName);
                Logging.Information("ContentSecurityPolicy: {csp}", Configuration["ContentSecurityPolicy:Policy"]);

                InitializeDbAsync(app).Wait();

                app.UseStaticFiles();

                app.UseIdentity();
                app.UseIdentityServer();

                // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

                app.UseMvcWithDefaultRoute();
                //app.UseMvc(routes =>
                //{
                //    routes.MapRoute(
                //        name: "default",
                //        template: "{controller=Home}/{action=Index}/{id?}");
                //});
            }
        }

        private async Task AddUserAsync(string firstName, string surname, string email, string password, string[] roles, UserManager<SAEONUser> userManager)
        {
            using (Logging.MethodCall(GetType(), new ParameterList { { "FirstName", firstName }, { "Surname", surname }, { "Email", email } }))
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

        private async Task InitializeDbAsync(IApplicationBuilder app)
        {
            using (Logging.MethodCall(GetType()))
            {
                using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                    scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
                    scope.ServiceProvider.GetRequiredService<SAEONDbContext>().Database.Migrate();

                    var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

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

                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<SAEONRole>>();
                    string[] roles = new string[] { "SAEON.Admin", "SAEON.Identity.Service", "SAEON.Observations.Admin"};
                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            var identityRole = new SAEONRole { Name = role };
                            await roleManager.CreateAsync(identityRole);
                            await roleManager.AddClaimAsync(identityRole, new Claim(ClaimTypes.Role, role));
                        }
                    }
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SAEONUser>>();
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
}
