using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using Thoth.Infrastructure;
using Thoth.Services;

namespace Thoth
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMemoryCache();
            services.AddScoped<ICachedResponseService, CachedResponseService>();
            services.AddScoped<ICacheProvider, CacheProvider>();

            // In localhost the https calls generate an error because the SSL validate fails.
            if (bool.Parse(Configuration["Configuration:Insecure"]))
            {
                services.AddHttpClient(Client.Insecure).ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                });
            }
            else
            {
                services.AddHttpClient(Client.Secure).ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler();
                });
            }

            services.AddSingleton<IClient, Client>();
            services.AddScoped<IGateway, Gateway>();

            // If using Kestrel:
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IGateway gateway)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            bool anyOrigin = false;
            bool.TryParse(Configuration["Configuration:AnyOrigin"], out anyOrigin);

            app.UseCors(x => x
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => anyOrigin) // allow any origin
               .AllowCredentials()); // allow credentials

            Router router = new Router(Configuration["Configuration:Routes"], gateway);
            app.Run(async (context) =>
            {
                var content = await router.RouteRequest(context.Request);
                context.Response.StatusCode = (int)content.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(content.Body);
            });
        }
    }
}