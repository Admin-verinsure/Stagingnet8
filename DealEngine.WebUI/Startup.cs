using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.HttpOverrides;
using DealEngine.Infrastructure.AppInitialize.Nhibernate;
using DealEngine.Infrastructure.AppInitialize.BaseLdapPackage;
using DealEngine.Infrastructure.AppInitialize.Services;
using DealEngine.Infrastructure.AppInitialize.Repositories;
using DealEngine.WebUI.Models;
using Microsoft.Extensions.Hosting;
using DealEngine.Infrastructure.AppInitialize;
using ElmahCore.Mvc;
using Microsoft.AspNetCore.Localization;
using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DealEngine.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LogoutPath = "/Account/Logout/";
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddServices();
            services.AddControllersWithViews();
            services.AddRouting();
            services.AddRazorPages();
            services.AddNHibernate();
            services.AddIdentityExtentions();
            services.AddSingleton(MapperConfig.DefaultProfile());
            services.AddLogging();
            services.AddConfig();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                //https://stackoverflow.com/questions/41289737/get-the-current-culture-in-a-controller-asp-net-core
                options.DefaultRequestCulture = new RequestCulture(culture: "en-NZ", uiCulture: "en-NZ");
            });
            // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-3.1
            services.ConfigureApplicationCookie(options =>
            {
                // options.AccessDeniedPath = "/Identity/Account/AccessDenied";     not implemented
                options.Cookie.Name = "DealEngine";
                options.Cookie.HttpOnly = true;
                options.Cookie.MaxAge = TimeSpan.FromMinutes(40);
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Account/Login";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });
            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddRepositories();
            services.AddBaseLdap();
            services.AddElmah(options =>
            {
                options.Path = @"c078b2de-f512-4225-90e8-90f8e17ac70b";
            });
            services.AddBaseLdapPackage();
            services.AddResponseCaching();
            services.AddMvc()
               .AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.IOTimeout = TimeSpan.FromMinutes(120);
            });
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(90);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();              
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");                
                app.UseHsts();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRequestLocalization();
            app.UseElmah();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();                
            });
        }
    }

}

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        if (context != null)
        {
            #region Other Headers that can be used
            //// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
            //// TODO Change the value depending of your needs
            //context.Response.Headers.Add("referrer-policy", new StringValues("strict-origin-when-cross-origin"));

            //// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
            //context.Response.Headers.Add("x-content-type-options", new StringValues("nosniff"));

            //// https://security.stackexchange.com/questions/166024/does-the-x-permitted-cross-domain-policies-header-have-any-benefit-for-my-websit
            //context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", new StringValues("none"));

            //// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-XSS-Protection
            //context.Response.Headers.Add("x-xss-protection", new StringValues("1; mode=block"));

            //// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Expect-CT
            //// You can use https://report-uri.com/ to get notified when a misissued certificate is detected
            //context.Response.Headers.Add("Expect-CT", new StringValues("max-age=0, enforce, report-uri=\"https://example.report-uri.com/r/d/ct/enforce\""));

            //// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Feature-Policy
            //// https://github.com/w3c/webappsec-feature-policy/blob/master/features.md
            //// https://developers.google.com/web/updates/2018/06/feature-policy
            //// TODO change the value of each rule and check the documentation to see if new features are available
            //context.Response.Headers.Add("Feature-Policy", new StringValues(
            //    "accelerometer 'none';" +
            //    "ambient-light-sensor 'none';" +
            //    "autoplay 'none';" +
            //    "battery 'none';" +
            //    "camera 'none';" +
            //    "display-capture 'none';" +
            //    "document-domain 'none';" +
            //    "encrypted-media 'none';" +
            //    "execution-while-not-rendered 'none';" +
            //    "execution-while-out-of-viewport 'none';" +
            //    "gyroscope 'none';" +
            //    "magnetometer 'none';" +
            //    "microphone 'none';" +
            //    "midi 'none';" +
            //    "navigation-override 'none';" +
            //    "payment 'none';" +
            //    "picture-in-picture 'none';" +
            //    "publickey-credentials-get 'none';" +
            //    "sync-xhr 'none';" +
            //    "usb 'none';" +
            //    "wake-lock 'none';" +
            //    "xr-spatial-tracking 'none';"
            //    ));
            #endregion

            #region Clickjacking Reasoning/Solution
            // There are two options to protect against Clickjacking(to prevent a resource from being improperly framed):

            // • The Content-Security - Policy header:
            // Content - Security - Policy: frame - ancestors 'none' | 'self' | ref.CSP2 source - list

            // • The X-Frame - Options header:
            // X - Frame - Options: DENY | SAMEORIGIN | ALLOW - FROM origin

            // CSP is the preferred solution.X - Frame - Options is widely supported by user-agents, but is deprecated for the more flexible CSP.
            #endregion

            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
            context.Response.Headers.Add("x-frame-options", new StringValues("DENY"));

            // https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
            // https://content-security-policy.com/unsafe-inline/
            context.Response.Headers.Add("Content-Security-Policy", new StringValues(
                "base-uri 'self';" +
                "block-all-mixed-content;" +
                "default-src 'self';" +
                "frame-ancestors 'none';" +
                "font-src 'self' https://fonts.gstatic.com https://maxcdn.bootstrapcdn.com https://fonts.googleapis.com ;" +
                "img-src 'self' data: https:;" +
                "script-src 'self' 'unsafe-inline';" +
                "style-src 'self' 'unsafe-inline' https://maxcdn.bootstrapcdn.com https://fonts.googleapis.com;" +
                "upgrade-insecure-requests;"

                #region Other Directives that can be used
                //"child-src 'none';" +
                //"connect-src 'self';" +
                //"object-src 'self';" +
                //"form-action 'self' ;" +
                //"frame-src 'none';" +
                //"manifest-src 'none';" +
                //"media-src 'none';" +
                //"sandbox allow-scripts allow-forms;" +
                //"script-src-elem 'self' 'unsafe-inline';" +
                //"style-src-attr 'self' 'unsafe-inline';" +
                //"style-src-elem 'self' 'unsafe-inline' https://maxcdn.bootstrapcdn.com  ;" +
                //"worker-src 'self';"
                #endregion

            ));
        }
    return _next(context);
    }
}