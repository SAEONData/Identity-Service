using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using PaulMiami.AspNetCore.Mvc.Recaptcha;
using SAEON.Identity.Service.Config;
using SAEON.Identity.Service.Data;
using SAEON.Identity.Service.Services;
using SAEON.Identity.Service.UI;
using SAEON.Logs;
using System;
using System.IO;
using System.Reflection;

namespace SAEON.Identity.Service
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public bool HTTPSEnabled { get; private set; } = false;


        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
            HTTPSEnabled = Convert.ToBoolean(Configuration["Application:HTTPSEnabled"] ?? "false");

            Logging
                .CreateConfiguration("Logs/SAEON.Identity.Service.txt", configuration)
                .Create();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (Logging.MethodCall(GetType()))
            {
                Logging.Information("Configuring services HTTPS: {HTTPSEnabled}", HTTPSEnabled);
                services.AddApplicationInsightsTelemetry();
                if (!Environment.IsDevelopment())
                {
                    if (HTTPSEnabled)
                    {
                        services.AddHttpsRedirection(options =>
                        {
                            options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                            options.HttpsPort = 443;
                        });
                    }
                }

                var connectionString = Configuration.GetConnectionString("IdentityService");
                services.AddAuthentication().AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                });
                var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
                services.AddDbContext<SAEONDbContext>(options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationsAssembly).EnableRetryOnFailure()));
                services
                    .AddIdentity<SAEONUser, SAEONRole>(config =>
                    {
                        config.User.RequireUniqueEmail = true;
                        config.SignIn.RequireConfirmedEmail = true; //prevents registered users from logging in until their email is confirmed
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
                    .AddEntityFrameworkStores<SAEONDbContext>()
                    .AddDefaultTokenProviders();
                services
                    .AddIdentityServer(config =>
                    {
                        config.Events = new EventsOptions
                        {
                            RaiseErrorEvents = true,
                            RaiseFailureEvents = true,
                            RaiseInformationEvents = true,
                            //RaiseSuccessEvents = true
                        };
                    })
                    .AddConfigurationStore(options =>
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly).EnableRetryOnFailure());
                    })
                    .AddOperationalStore(options =>
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly).EnableRetryOnFailure());

                        // this enables automatic token cleanup. this is optional.
                        options.EnableTokenCleanup = true;
                        options.TokenCleanupInterval = Convert.ToInt32(Configuration["Service:TokenCleanup"]); // interval in seconds
                    })
                    .AddAspNetIdentity<SAEONUser>()
                    .AddProfileService<IdentityProfileService>()
                    .AddSigningCredential(Cert.Load());

                //services.AddMvc(options =>
                //{
                //    options.Filters.Add<SecurityHeadersAttribute>();
                //});

                services.AddMvc();

                services.AddRecaptcha(new RecaptchaOptions
                {
                    SiteKey = Configuration["Recaptcha:SiteKey"],
                    SecretKey = Configuration["Recaptcha:SecretKey"]
                });

                services.AddLogging();
                services.AddCors();

                services.AddSingleton<IConfiguration>(Configuration);

                // Add application services.
                services.AddTransient<IEmailSender, EmailSender>();
                services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("SendGrid"));
            }

            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            using (Logging.MethodCall(GetType()))
            {
                Logging.Verbose("Configure: {IsDevelopment}", env.IsDevelopment());
                //app.UseApplicationInsights();
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseDatabaseErrorPage();
                }
                else
                {
                    if (HTTPSEnabled)
                    {
                        app.UseHttpsRedirection();
                    }
                    app.UseExceptionHandler("/Home/Error");
                }
                Logging.Information("Environment: {environment}", env.EnvironmentName);
                app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());
                app.UseStaticFiles();
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "node_modules")),
                    RequestPath = "/node_modules"
                });

                app.UseIdentityServer();
                app.UseAuthentication();

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

    }
}
