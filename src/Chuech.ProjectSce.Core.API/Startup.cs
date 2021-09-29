using System.Text.Json;
using System.Text.Json.Serialization;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Files;
using Chuech.ProjectSce.Core.API.Features.Files.AccessLinks;
using Chuech.ProjectSce.Core.API.Features.Files.Data;
using Chuech.ProjectSce.Core.API.Features.Files.Storage;
using Chuech.ProjectSce.Core.API.Features.Files.UserTrackingStorage;
using Chuech.ProjectSce.Core.API.Features.Groups;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Resources.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;
using Chuech.ProjectSce.Core.API.Features.Users;
using Chuech.ProjectSce.Core.API.Infrastructure.Authorization;
using Chuech.ProjectSce.Core.API.Infrastructure.HttpPipeline;
using Chuech.ProjectSce.Core.API.Infrastructure.MediatRPipeline;
using Chuech.ProjectSce.Identity.Grpc;
using FluentValidation.AspNetCore;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityModel.Client;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration.ScopeProviders;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;

namespace Chuech.ProjectSce.Core.API
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
            AddApiServices(services);
            AddApiDocumentationServices(services);
            AddAuthenticationServices(services);
            ConfigureFileOptions(services);

            AddDataServices(services);
            AddEventBusServices(services);

            AddCustomServices(services);
            AddExternalServices(services);

            services.AddHttpContextAccessor();
            services.AddMediatR(typeof(Startup).Assembly);
        }

        private void ConfigureFileOptions(IServiceCollection services)
        {
            services.Configure<FormOptions>(options =>
            {
                var maximum = Configuration.GetValue("Files:MaximumSize", ServerConstants.MinimalMaxRequestSize);
                options.MultipartBodyLengthLimit = Math.Max(maximum, ServerConstants.MinimalMaxRequestSize);
            });

            services.AddOptions<FilesOptions>()
                .Bind(Configuration.GetSection(FilesOptions.ConfigurationSection))
                .ValidateDataAnnotations();
        }

        private void AddDataServices(IServiceCollection services)
        {
            services.AddDbContext<CoreContext>(
                options => options
                    .UseNpgsql(Configuration.GetConnectionString("Core")));

            services.AddDbContext<FileStorageContext>(
                options => options
                    .UseNpgsql(Configuration.GetConnectionString("FileStorage")));

            services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.AddScoped(s => s.GetRequiredService<IRedisDatabaseProvider>().GetDatabase());

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
        }

        private void AddEventBusServices(IServiceCollection services)
        {
            services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((context, mqConfig) =>
                {
                    mqConfig.UseInMemoryOutbox();

                    mqConfig.Host(Configuration.GetValue<string>("RabbitMQHost"), "/", h =>
                    {
                        h.Username(Configuration.GetValue<string>("RabbitMQUser"));
                        h.Password(Configuration.GetValue<string>("RabbitMQPassword"));
                    });
                    mqConfig.ConfigureEndpoints(context);

                    mqConfig.UsePublishFilter(typeof(PassAuthenticationInfoFilter<>), context);
                    mqConfig.UseConsumeFilter(typeof(HydrateAuthenticationContextAcccessorFilter<>), context);
                });

                config.AddConsumers(typeof(Startup).Assembly);
            });

            services.AddMassTransitHostedService();
            services.AddGenericRequestClient();
        }

        private void AddExternalServices(IServiceCollection services)
        {
            var identityGrpcUrl = Configuration.GetValue<string>("IdentityGrpcUrl");
            services.AddGrpcClient<UserService.UserServiceClient>((_, options) =>
            {
                options.Address = new Uri(identityGrpcUrl);
            });
        }

        private void AddAuthenticationServices(IServiceCollection services)
        {
            // We should change this to JWTs.
            services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
                .AddOAuth2Introspection(options =>
                {
                    options.Authority = Configuration.GetValue<string>("IdentityUrlInternal");

                    options.ClientId = "core";
                    options.ClientSecret = "whatever_lul"; // TODO: Security concern, secrets in clear text

                    options.DiscoveryPolicy = new DiscoveryPolicy
                    {
                        // TODO: Security concern, HTTPS please
                        RequireHttps = false
                    };

                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(2);
                });
        }

        private static void AddApiDocumentationServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Chuech Core API",
                    Version = "v1"
                });
                
                c.DescribeAllParametersInCamelCase();

                // Avoid schema id collisions with nested types while keeping them short and easy to read.
                c.CustomSchemaIds(t =>
                {
                    var fullName = t.FullName?.Replace('+', '_'); // Underscores are easier to read than +
                    if (fullName is null)
                    {
                        return t.Name;
                    }

                    var @namespace = t.Namespace;
                    if (@namespace is not null)
                    {
                        return fullName[(@namespace.Length + 1)..]; // + 1 to remove the extra dot
                    }

                    return fullName;
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"The token used for authentication.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header

                        },
                        Array.Empty<string>()
                    }
                });
                
                c.CustomOperationIds(e =>
                {
                    var actionName = e.ActionDescriptor.RouteValues.TryGetValue("action", out var action)
                        ? action
                        : e.HttpMethod;

                    var controllerName = e.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller)
                        ? controller
                        : e.ActionDescriptor.DisplayName ?? "unknown";

                    return $"{controllerName}_{actionName}";
                });

                c.MapType<TimeSpan>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "period",
                    Example = new OpenApiString("PT13H37M1S")
                });
                
                c.MapType<RgbColor>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "hex-color",
                    Example = new OpenApiString("#FFFFFF")
                });
            });
        }

        private static void AddApiServices(IServiceCollection services)
        {
            services.AddControllers(options => { options.Filters.AddService<HandleExceptionsFilter>(); })
                .AddFluentValidation(config => { config.RegisterValidatorsFromAssemblyContaining<Startup>(); })
                .AddJsonOptions(options =>
                {
                    var converters = options.JsonSerializerOptions.Converters;
                    
                    converters.Add(new TimeSpanJsonConverter());
                    converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                });
        }

        private static void AddCustomServices(IServiceCollection services)
        {
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IInstitutionAuthorizationService, InstitutionAuthorizationService>();

            services.AddTransient<IResourceAuthorizationService, ResourceAuthorizationService>();
            
            services.AddTransient<HandleExceptionsFilter>();
            services.AddScoped<RequestContextAccessor>();
            
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionReportingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ProfilingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InstitutionAuthorizationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ResourceAuthorizationBehavior<,>));

            services.AddScoped<IFileStorage, FileSystemStorage>();
            services.AddScoped<IFileAccessLinkGenerator, LocalFileAccessLinkGenerator>();
            services.AddScoped<FileChecker>();

            services.AddScoped<IUserFileStorageLimitProvider, UserFileStorageLimitProvider>();
            services.AddScoped<IUserTrackingFileStorage, UserTrackingFileStorage>();

            services.AddTransient(typeof(AuthBarrier<>));

            services.AddTransient<AccessibleUsersFromIdsQuery>();
            services.AddScoped<IInstitutionGatewayService, InstitutionGatewayService>();

            services.AddScoped<ISpaceAuthorizationService, SpaceAuthorizationService>();
            services.AddScoped<ISpaceUserAuthorizationInfoCache, RedisSpaceUserAuthorizationInfoCache>();
            services.AddScoped<IIndividualSpaceMemberCalculator, IndividualSpaceMemberCalculator>();
            services.AddScoped<RemoveMember.GenericRemover>();

            services.AddScoped<MTRequestAuthenticationContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chuech.ProjectSce.Core.API v1"));
            }
            
            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}