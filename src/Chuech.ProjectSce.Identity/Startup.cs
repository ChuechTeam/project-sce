using System.Linq;
using Chuech.ProjectSce.Identity.Data;
using Chuech.ProjectSce.Identity.Grpc;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Chuech.ProjectSce.Identity
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            ConfigureDatabase(services);
            ConfigureIdentity(services);
            ConfigureEventBus(services);

            services.AddGrpc();
        }

        private void ConfigureEventBus(IServiceCollection services)
        {
            services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((_, mqConfig) =>
                {
                    mqConfig.Host(Configuration.GetValue<string>("RabbitMQHost"), "/", h =>
                    {
                        h.Username(Configuration.GetValue<string>("RabbitMQUser"));
                        h.Password(Configuration.GetValue<string>("RabbitMQPassword"));
                    });
                });
            });
            services.AddMassTransitHostedService();
        }

        private void ConfigureIdentity(IServiceCollection services)
        {
            services.AddAuthentication();
            
            services.AddIdentity<UserAccount, IdentityRole>(options =>
                {
                    // Options there
                })
                .AddEntityFrameworkStores<AppIdentityContext>()
                .AddDefaultTokenProviders();

            var clients = IdentityConfiguration.GetClients(Configuration, Environment).ToArray();
            
            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                    options.EmitStaticAudienceClaim = true;
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = db =>
                        db.UseNpgsql(Configuration.GetConnectionString("OperationalIdentity"),
                                b => b.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                            .UseSnakeCaseNamingConvention();
                })
                .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
                .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
                .AddInMemoryClients(clients)
                .AddInMemoryApiResources(IdentityConfiguration.ApiResources)
                .AddAspNetIdentity<UserAccount>();

            // not recommended for production - you need to store your key material somewhere secure
            // TODO: Security concern - Secret keys
            builder.AddDeveloperSigningCredential();

            services.Decorate<IUserClaimsPrincipalFactory<UserAccount>, UserAccountClaimsPrincipalFactory>();
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<AppIdentityContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Identity"))
                    .UseSnakeCaseNamingConvention());
            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();

                var grpcPort = Configuration.GetValue<int?>("GrpcPort") ?? 751;
                endpoints.MapGrpcService<GrpcUserService>().RequireHost($"*:{grpcPort}");
            });
        }
    }
}