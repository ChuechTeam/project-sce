using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.InfrastructureTools;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Chuech.ProjectSce.Core.API;

public class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        try
        {
            var host = CreateHostBuilder(args).Build();

            host.MigrateDatabase<CoreContext>();

            host.Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog((context, config) =>
            {
                config
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware", LogEventLevel.Fatal)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code, restrictedToMinimumLevel: LogEventLevel.Warning);

                var seqUrl = context.Configuration.GetValue<string>("SeqUrl");
                if (!string.IsNullOrWhiteSpace(seqUrl))
                {
                    config.WriteTo.Seq(seqUrl, restrictedToMinimumLevel: LogEventLevel.Debug);
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel((hosting, options) =>
                {
                    var maxFileSize = hosting.Configuration.GetValue("Files:MaximumSize",
                        ServerConstants.MinimalMaxRequestSize);

                    options.Limits.MaxRequestBodySize = Math.Max(ServerConstants.MinimalMaxRequestSize, maxFileSize);
                });
                webBuilder.UseStartup<Startup>();
            });
    }
}