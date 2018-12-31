using System;
using System.IO;
using System.Linq;
using Briver.Framework;
using Briver.Logging;
using Briver.Web.ModelBinding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: CompositionSupported]

namespace Briver.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddMvcOptions(options =>
                {
                    options.ModelBinderProviders.Insert(0, new JsonModelBinderProvider());
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            services.AddApiVersioning(options =>
            {
                //options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("api", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    //Version = "v1",
                    Title = "Briver.WebApp",
                    //Contact = new Swashbuckle.AspNetCore.Swagger.Contact
                    //{
                    //    Name = "陈勇江",
                    //    Email = "chenyj796@126.com",
                    //    Url = "https://github.com/chenyj796/Briver.NetCore.git"
                    //},
                    //Description = "测试网站",
                });


                s.ResolveConflictingActions(actions => actions.First());
                s.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Briver.WebApp.xml"));
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                s.SwaggerEndpoint("/swagger/api/swagger.json", "Briver.WebApp");
            });
        }
    }
}
