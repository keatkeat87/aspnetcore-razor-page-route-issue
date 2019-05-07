using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Project
{
    public class RouteLanguageConstraint : IRouteConstraint
    {
        public static readonly string segmentName = "language";

        public bool Match(HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (routeKey == null) throw new ArgumentNullException(nameof(routeKey));
            if (values == null) throw new ArgumentNullException(nameof(values));

            if (values.TryGetValue(routeKey, out var routeValue))
            {
                string language = routeValue.ToString();
                return language == "zh-Hans" || language == "en-US";
            }

            return false;
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RouteOptions>(routeOptions =>
            {
                routeOptions.ConstraintMap.Add("language", typeof(RouteLanguageConstraint));
            });

            services.AddLocalization();
            services.AddMvc()
              .AddViewLocalization()
              .AddDataAnnotationsLocalization()
              .AddRazorPagesOptions(options =>
               {
                   options.Conventions.AddPageRoute("/About/About", "/about-us");
                   options.Conventions.AddPageRoute("/About/About", "/{language:language}/about-us");
               })
              .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("zh-Hans"),
            };

            app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("zh-Hans");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(context =>
                {
                    return Task.FromResult(new ProviderCultureResult("zh-Hans"));
                }));
            });

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
