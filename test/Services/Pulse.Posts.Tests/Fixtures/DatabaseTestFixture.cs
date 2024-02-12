using System.Data;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Posts.Data;
using Pulse.Posts.Migrations;
using Pulse.Posts.Tests.Data;

namespace Pulse.Posts.Tests.Fixtures;

public class DatabaseFixture : IDisposable
{
    public readonly string DatabaseId = Guid.NewGuid().ToString();
    internal PostsContext Posts { get; init; }

    private string ConnectionString => $"Data Source={DatabaseId};Mode=Memory;Cache=Shared";

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<PostsContext>()
            .UseInMemoryDatabase(DatabaseId)
            .Options;

        Posts = new PostsContext(options);

        using (var services = CreateServices())
        using (var scope = services.CreateScope())
        {
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }

    public void Dispose()
    {
        using (var services = CreateServices())
        using (var scope = services.CreateScope())
        {
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateDown(0);
        }
    }

    private ServiceProvider CreateServices()
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
                rb.AddSQLite()
                    .WithGlobalConnectionString(ConnectionString)
                    .ScanIn(typeof(AddPostsTable).Assembly)
                    .For.Migrations()
            )
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
