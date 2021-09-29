using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Files.Data;
using Chuech.ProjectSce.InfrastructureTools;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Chuech.ProjectSce.Core.API
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                host.MigrateDatabase<CoreContext>();
                host.MigrateDatabase<FileStorageContext>();

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
                .UseSerilog()
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
}