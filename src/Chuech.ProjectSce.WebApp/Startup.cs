using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VueCliMiddleware;

namespace Chuech.ProjectSce.WebApp
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
            services.AddControllers();
            services.AddSpaStaticFiles(options => options.RootPath = "project-sce-app/dist");
            services.AddRazorPages();

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AuthServerCors", policy =>
                {
                    policy.WithOrigins(Configuration.GetValue<string>("IdentityUrlExternal"))
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("AuthServerCors");

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/auth-info", async context =>
                {
                    await context.Response.WriteAsJsonAsync(new
                    {
                        AuthUrl = Configuration.GetValue<string>("IdentityUrlExternal")
                    });
                });
                endpoints.MapRazorPages();
            });
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "project-sce-app";

                if (env.IsDevelopment())
                {
                    const int spaPort = 8754;

                    var isRunningInContainer = Configuration.GetValue<bool?>("DOTNET_RUNNING_IN_CONTAINER") ?? false;
                    if (isRunningInContainer)
                    {
                        spa.UseProxyToSpaDevelopmentServer(Configuration.GetValue<string>("DevServerUrl"));
                    }
                    else
                    {
                        spa.UseVueCli(port: spaPort);
                    }
                }
            });
        }
    }
}