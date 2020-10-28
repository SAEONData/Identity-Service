using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using PaulMiami.AspNetCore.Mvc.Recaptcha;
using SAEON.Identity.Service.Config;
using SAEON.Identity.Service.Data;
using SAEON.Identity.Service.Services;
using SAEON.Identity.Service.UI;
using SAEON.Logs;
using System;
using System.IO;

namespace SAEON.Identity.Service
{
    public class Startup
    {
        public IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (SAEONLogs.MethodCall(GetType()))
            {
                services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:InstrumentationKey"]);
                var connectionString = Configuration.GetConnectionString("IdentityService");
                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });
                services.AddAuthentication().AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                });
                services.AddDbContext<SAEONDbContext>(options => options.UseSqlServer(connectionString, b => b.EnableRetryOnFailure()));
                services
                    .AddIdentity<SAEONUser, SAEONRole>(config =>
                    {
                        config.User.RequireUniqueEmail = true;
                        config.SignIn.RequireConfirmedEmail = true; //prevents registered users from SAEONLogs in until their email is confirmed
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
                            builder.UseSqlServer(connectionString, b => b.EnableRetryOnFailure());
                    })
                    .AddOperationalStore(options =>
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseSqlServer(connectionString, b => b.EnableRetryOnFailure());

                        // this enables automatic token cleanup. this is optional.
                        options.EnableTokenCleanup = true;
                        options.TokenCleanupInterval = Convert.ToInt32(Configuration["Service:TokenCleanup"]); // interval in seconds
                    })
                    .AddAspNetIdentity<SAEONUser>()
                    .AddProfileService<IdentityProfileService>()
                    .AddSigningCredential(Cert.Load());

                services.AddMvc();

                services.AddRecaptcha(new RecaptchaOptions
                {
                    SiteKey = Configuration["Recaptcha:SiteKey"],
                    SecretKey = Configuration["Recaptcha:SecretKey"]
                });

                //services.AddCors();
                //services.AddSingleton<IConfiguration>(Configuration);

                // Add application services.
                services.AddTransient<IEmailSender, EmailSender>();
                services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("SendGrid"));
            }

            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (SAEONLogs.MethodCall(GetType()))
            {
                SAEONLogs.Verbose("Configure: {IsDevelopment}", env.IsDevelopment());
                SAEONLogs.Information("Environment: {environment}", env.EnvironmentName);
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "node_modules")),
                    RequestPath = "/node_modules"
                });

                app.UseCookiePolicy();
                //app.UseResponseCaching();
                //app.UseResponseCompression();
                app.UseRouting();
                app.UseIdentityServer();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapDefaultControllerRoute();
                });
            }
        }

    }
}
