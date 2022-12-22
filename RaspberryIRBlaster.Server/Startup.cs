using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace RaspberryIRBlaster.Server
{
#pragma warning disable CA1822 // Mark members as static
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
            services.AddControllers(options =>
                options.Filters.Add(typeof(HttpExceptionFilter)));

            if (Program.Config.GeneralConfig.IdleShutdownMins > 0)
            {
                services.AddHostedService<Application.IdleShutdown>();
            }
            services.AddHostedService<Application.IRTransmitter>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policyBuilder =>
                {
                    if (Program.Config.GeneralConfig.CorsOrigins?.Length > 0)
                    {
                        policyBuilder.WithOrigins(Program.Config.GeneralConfig.CorsOrigins);
                    }
                    policyBuilder.WithMethods("GET", "POST");
                    policyBuilder.WithHeaders(HeaderNames.ContentType);
                    policyBuilder.SetPreflightMaxAge(TimeSpan.FromMinutes(1));
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (Program.Config.GeneralConfig.IdleShutdownMins > 0)
            {
                app.Use(async (context, next) =>
                {
                    if (Application.IdleShutdown.Instance == null)
                    {
                        throw new NullReferenceException("The idle shutdown service does not exist.");
                    }
                    Application.IdleShutdown.Instance.Activity();

                    await next();
                });
            }

            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "GET" && context.Request.Path == "/")
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("This is the Raspberry IR Blaster .NET REST server.");
                }
                else
                {
                    await next();
                }
            });

            app.UseRouting();

            app.UseCors(); // The call to UseCors must be placed after UseRouting, but before UseAuthorization. https://learn.microsoft.com/en-us/aspnet/core/security/cors

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
#pragma warning restore CA1822
}
