using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;

namespace Chuech.ProjectSce.InfrastructureTools
{
    public static class HostDatabaseMigrationExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
            
            const int Attempts = 6;
            
            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetry(Attempts, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (exception, sleepDuration, retryCount, _) =>
                    {
                        logger.LogWarning("Failed to migrate database with message '{Message}' " +
                                          "(Attempt {CurrentAttempt}/{MaxAttempts}; retrying in {Sleep}ms)", 
                            exception.Message, retryCount, Attempts, sleepDuration.TotalMilliseconds);
                    }
                );

            try
            {
                policy.Execute(() =>
                {
                    context.Database.Migrate();
                    
                    var connection = context.Database.GetDbConnection();
                    if (connection is NpgsqlConnection postgresConnection)
                    {
                        postgresConnection.Open();
                        postgresConnection.ReloadTypes();
                    }
                });
            }
            catch (Exception)
            {
                logger.LogError("Database migration failed for context {DbName}", typeof(TContext).Name);
                throw;
            }
            
            logger.LogInformation("Successfully migrated database for context {DbName}", typeof(TContext).Name);

            return host;   
        }
    }
}