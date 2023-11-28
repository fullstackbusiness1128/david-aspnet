using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FunTokenz.Data;
using FunTokenz.Filter;
using FunTokenz.Models.Data;
using FunTokenz.Services;
using Stripe;
using FunTokenz.Models.Business;
using Newtonsoft.Json.Serialization;


namespace FunTokenz
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


            services.ConfigureApplicationCookie(options => options.LoginPath = "/customers/signin");
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());




            //Add Logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            // Add application services.
            services.AddTransient<IRDS, RDS>();
            services.AddTransient<IIdentity, Identity>();
            services.AddTransient<IComms, Comms>();
            services.AddTransient<IS3, S3>();
            services.AddScoped<ValidateSessionAttribute>();
            services.AddScoped<MustBeSignedIn>();
            services.AddScoped<MustBeSignedOut>();
            services.AddSession(options => { options.IdleTimeout = TimeSpan.FromHours(24); options.Cookie.HttpOnly = true; });
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<FTConfig>(ftConfig);
            

            //Antiforgery
            services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

            //STRIPE
           
            services.Configure<StripeSettings>(Configuration.GetSection("FTConfig"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
                app.UseExceptionHandler("/Oops");
                app.UseHsts();
            }


            //STRIPE
            StripeConfiguration.ApiKey = Configuration.GetSection("FTConfig")["StripeSecretKey"];

            app.UseHttpsRedirection();
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

            provider.Mappings[".webmanifest"] = "application/manifest+json";
            app.UseStaticFiles(new StaticFileOptions() { ContentTypeProvider = provider });

            //app.UseStaticFiles();
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
