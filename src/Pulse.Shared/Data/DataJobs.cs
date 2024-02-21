using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Pulse.Shared.Data;

public static class DataJobs
{
    public static void MigrateDatabase(string connectionString, Assembly migrationAssembly)
    {
        using (var serviceProvider = CreateServices(connectionString, migrationAssembly))
        using (var scope = serviceProvider.CreateScope())
        {
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }

    private static ServiceProvider CreateServices(
        string connectionString,
        Assembly migrationAssembly
    ) =>
        new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
                rb.AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(migrationAssembly)
                    .For.Migrations()
            )
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);
}
