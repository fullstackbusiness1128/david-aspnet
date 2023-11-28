using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CMS.Data;
using CMS.Models;
using CMS.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using CMS.Filters;
using CMS.Services.RDS;
using Microsoft.AspNetCore.Http.Features;
using CMS.Services.Cache;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using CMS.Models.BusinessModels;
using Stripe;

namespace CMS
{

    public class StripeSettings
    {
        public string StripeSecretKey { get; set; }
        public string StripePublishableKey { get; set; }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        private IHostingEnvironment CurrentEnvironment { get; set; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // FTConfig
            services.Configure<FTConfig>(Configuration.GetSection(nameof(FTConfig)));

            // Make Available Now
            services.AddSingleton(Configuration);

            FTConfig ftConfig = new FTConfig();
            ftConfig = Configuration.GetSection(nameof(FTConfig)).Get<FTConfig>();

            //Form Uploads
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
                x.MultipartHeadersLengthLimit = int.MaxValue;
                x.ValueLengthLimit = int.MaxValue;
                x.KeyLengthLimit = int.MaxValue;
            });

            // SQL
            //string ConnectionString;
            //ConnectionString = ftConfig.DBconnectionString;
            
            string ConnectionString = Configuration["FTConfig:DBConnection"];


            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(ConnectionString));

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //var cookieAuthOptions = new CookieAuthenticationOptions()
            //{
            //    LoginPath = "/Account/LogIn", 
            //    LogoutPath = "/Account/LogOff",
            //    ExpireTimeSpan = TimeSpan.FromDays(150)
            //};
            //services.ConfigureApplicationCookie(c => c.LoginPath = cookieAuthOptions.LoginPath);
            services.ConfigureApplicationCookie(c => c.LoginPath = "/Account/LogIn");
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // CACHE
            //var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(ftConfig.CacheConnectionString);
            //services.AddDataProtection()
            //.PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = ftConfig.CacheConnectionString;
            //    options.InstanceName = ftConfig.CacheInstance;
            //});

            // MVC
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //Add Logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            // Add application services.
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IRDS, RDS>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddScoped<UserSignedOutOnly>();
            services.AddScoped<ValidateSessionAttribute>();
            services.AddSession(options => { options.IdleTimeout = TimeSpan.FromHours(24); options.Cookie.HttpOnly = true; });
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<FTConfig>(ftConfig);

            // AWS
            // REENTER IAM ROLE FOR VISUALSTUDIO POST MIGRATION
            // Development (Local) - Credentials are stored with AWS CLI Secret Store - In the users appdata folder - Inserted via CLI - Uses IAM Keys
            // Staging - Credentials are stored as an EC2 Service Role that have related policies attached
            // Production - Credentials are stored as an EC2 Service Role that have related policies attached
            // AWS Services automatically can retrieve the requested creds from these stores
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();

            //Antiforgery
            services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

            //STRIPE
            services.Configure<StripeSettings>(Configuration.GetSection("FTConfig"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                ForwardLimit = null,
                RequireHeaderSymmetry = false
            });
            app.UseHttpMethodOverride();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //STRIPE
            StripeConfiguration.ApiKey = Configuration.GetSection("FTConfig")["StripeSecretKey"];

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
