using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Posts.Data;
using Pulse.Posts.Migrations;

namespace Pulse.Posts.Tests.Fixtures;

public class DatabaseFixture : IDisposable
{
    public readonly string DatabaseId = Guid.NewGuid().ToString();
    internal PostsContext Posts { get; init; }
    
    // Store a reference to the connection that we use for migrations and DbContext
    private readonly SqliteConnection _connection;

    public DatabaseFixture()
    {
        var connectionString = $"DataSource=file:{DatabaseId}?mode=memory&cache=shared";
        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        using (var services = CreateServices())
        using (var scope = services.CreateScope())
        {
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        var options = new DbContextOptionsBuilder<PostsContext>()
            .UseSqlite(_connection)
            .Options;

        Posts = new PostsContext(options);
    }

    public void Dispose()
    {
        Posts?.Dispose();
        
        _connection?.Close();
        _connection?.Dispose();
    }

    private ServiceProvider CreateServices()
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
                rb.AddSQLite()
                    .WithGlobalConnectionString(_connection.ConnectionString)
                    .ScanIn(typeof(AddPostsTable).Assembly)
                    .For.Migrations()
            )
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
