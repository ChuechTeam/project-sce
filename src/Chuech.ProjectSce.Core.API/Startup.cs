using System.Text.Json;
using System.Text.Json.Serialization;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Groups;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Resources;
using Chuech.ProjectSce.Core.API.Features.Resources.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;
using Chuech.ProjectSce.Core.API.Features.Users;
using Chuech.ProjectSce.Core.API.Infrastructure.ApiDocumentation;
using Chuech.ProjectSce.Core.API.Infrastructure.Authentication;
using Chuech.ProjectSce.Core.API.Infrastructure.HttpPipeline;
using Chuech.ProjectSce.Core.API.Infrastructure.MediatRPipeline;
using Chuech.ProjectSce.Identity.Grpc;
using FluentValidation.AspNetCore;
using GreenPipes;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityModel.Client;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MicroElements.Swashbuckle.NodaTime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Minio;
using AuthenticationSchemeOptions = Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions;
using NodaTime.Serialization.JsonNet;
using NodaTime.Serialization.SystemTextJson;
using Serilog;

namespace Chuech.ProjectSce.Core.API;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    private bool IsImpersonateEnabled => Environment.IsDevelopment();

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        AddApiServices(services);
        AddApiDocumentationServices(services);
        AddAuthenticationServices(services);
        ConfigureFileOptions(services);

        AddDataServices(services);
        AddEventBusServices(services);
        AddFileServices(services);

        AddCustomServices(services);
        AddExternalServices(services);

        AddPolicies(services);

        services.AddSingleton<IClock>(SystemClock.Instance);

        services.AddHttpContextAccessor();
        services.AddMediatR(typeof(Startup).Assembly);
    }

    private static void AddPolicies(IServiceCollection services) => services.AddSingleton<ChuechPolicyRegistry>();

    private void ConfigureFileOptions(IServiceCollection services)
    {
        services.Configure<FormOptions>(options =>
        {
            var maximum = Configuration.GetValue("Files:MaximumSize", ServerConstants.MinimalMaxRequestSize);
            options.MultipartBodyLengthLimit = Math.Max(maximum, ServerConstants.MinimalMaxRequestSize);
        });
    }

    private void AddDataServices(IServiceCollection services)
    {
        // The large connection pool can be a problem, but pgbouncer will take care of this.
        services.AddDbContextPool<CoreContext>(
            options => CoreContext.ConfigureOptions(options, Configuration.GetConnectionString("Core")),
            poolSize: 512);

        var redisDatabaseProvider = new RedisDatabaseProvider(Configuration);
        services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>(_ => redisDatabaseProvider);
        services.AddScoped(s => s.GetRequiredService<IRedisDatabaseProvider>().GetDatabase());

        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.ConnectionMultiplexerFactory = () => Task.FromResult(redisDatabaseProvider.GetMultiplexer());
        });
    }

    private void AddEventBusServices(IServiceCollection services)
    {
        services.AddMassTransit(config =>
        {
            config.AddDelayedMessageScheduler();

            config.UsingRabbitMq((context, mqConfig) =>
            {
                mqConfig.UseDelayedMessageScheduler();

                // Retry when any optimistic concurrency exception occurs.
                mqConfig.UseMessageRetry(retry =>
                {
                    retry.Handle<DbUpdateConcurrencyException>();
                    retry.Immediate(3);
                });

                // Retry when any other recoverable exception occurs for a short period of time.
                mqConfig.UseMessageRetry(retry =>
                {
                    retry.Ignore<ProjectSceException>();
                    retry.Ignore<NullReferenceException>();
                    retry.Intervals(5, 100, 1000);
                });

                mqConfig.UseMessageScope(context);

                mqConfig.UseInMemoryOutbox();

                mqConfig.Host(Configuration.GetValue<string>("RabbitMQHost"), "/", h =>
                {
                    h.Username(Configuration.GetValue<string>("RabbitMQUser"));
                    h.Password(Configuration.GetValue<string>("RabbitMQPassword"));
                });
                mqConfig.ConfigureEndpoints(context);

                mqConfig.ConfigureJsonSerializer(o => o.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
                mqConfig.ConfigureJsonDeserializer(o => o.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
            });

            config.SetEntityFrameworkSagaRepositoryProvider(x =>
            {
                x.ConcurrencyMode = ConcurrencyMode.Optimistic;
                x.ExistingDbContext<CoreContext>();
            });

            config.AddConsumers(typeof(Startup).Assembly);
            config.AddSagaStateMachines(typeof(Startup).Assembly);
        });

        services.AddMassTransitHostedService();
        services.AddGenericRequestClient();
    }

    private void AddFileServices(IServiceCollection services)
    {
        services.AddSingleton(new MinioClient(
            Configuration.GetValue<string>("MinioUrl"),
            Configuration.GetValue<string>("MinioUser"),
            Configuration.GetValue<string>("MinioPassword")));
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
        var authBuilder = services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
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

        if (IsImpersonateEnabled)
        {
            authBuilder.AddScheme<AuthenticationSchemeOptions, ImpersonateAuthenticationHandler>(
                "Impersonate", _ => { });

            services.AddAuthorization(options => options.DefaultPolicy =
                new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(OAuth2IntrospectionDefaults.AuthenticationScheme, "Impersonate")
                    .RequireAuthenticatedUser()
                    .Build());
        }
    }

    private void AddApiDocumentationServices(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Chuech Core API",
                Version = "v1"
            });

            c.DescribeAllParametersInCamelCase();
            c.ConfigureForNodaTimeWithSystemTextJson(configureSerializerOptions: ConfigureJsonOptions);

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

            if (IsImpersonateEnabled)
            {
                c.AddSecurityDefinition("Impersonate", new OpenApiSecurityScheme
                {
                    Description =
                        "The user id to impersonate with the following format: 'Impersonate [id]'. Development only!",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Impersonate"
                            },
                            Name = "Impersonate",
                            In = ParameterLocation.Header
                        },
                        Array.Empty<string>()
                    }
                });
            }

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

            c.SchemaFilter<JsonIgnoreSchemaFilter>();

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
            .AddJsonOptions(o => ConfigureJsonOptions(o.JsonSerializerOptions));
    }

    private static void ConfigureJsonOptions(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }

    private static void AddCustomServices(IServiceCollection services)
    {
        services.AddTransient<IAuthenticationService, HttpAuthenticationService>();
        services.AddScoped<InstitutionAuthorizationService>();

        services.AddScoped<ResourceAuthorizationService>();
        services.AddScoped<ResourcePublicationAuthorizationService>();
        services.AddScoped<ResourcePublicationValidator>();

        services.AddTransient<HandleExceptionsFilter>();
        services.AddScoped<RequestContextAccessor>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionReportingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ProfilingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InstitutionAuthorizationBehavior<,>));

        services.AddTransient(typeof(AuthBarrier<>));

        services.AddScoped<InstitutionGatewayService>();
        services.AddScoped<IInstitutionAuthorizationCache, RedisInstitutionAuthorizationCache>();
        services.Decorate<IInstitutionAuthorizationCache, ResilientInstitutionAuthorizationCache>();

        services.AddScoped<SpaceAuthorizationService>();
        services.AddScoped<ISpaceAuthorizationCache, RedisSpaceAuthorizationCache>();
        services.Decorate<ISpaceAuthorizationCache, ResilientSpaceAuthorizationCache>();
        services.AddScoped<IndividualSpaceMemberCalculator>();

        services.AddTransient<GroupUserPresenceValidator>();
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