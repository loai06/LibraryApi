using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Tests;

public sealed class Inbox : IEmailService
{
    public ConcurrentDictionary<string, string> Codes { get; } = new();
    public Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        Codes[to] = Regex.Match(htmlBody, @"<h2>([0-9]{6})</h2>").Groups[1].Value;
        return Task.CompletedTask;
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = "LibraryApiTests_" + Guid.NewGuid().ToString("N");
    public string SigningKey { get; } = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    public Inbox Inbox { get; } = new();
    public string ConnectionString { get; private set; } = "";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = SigningKey,
                ["Jwt:Issuer"] = "LibraryApiTests",
                ["Jwt:Audience"] = "LibraryApiTests"
            }));
        builder.ConfigureServices((context, services) =>
        {
            var connection = new SqlConnectionStringBuilder(
                context.Configuration.GetConnectionString("DefaultConnection"));
            connection.InitialCatalog = DatabaseName;
            connection.ConnectTimeout = 5;
            ConnectionString = connection.ConnectionString;
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(ConnectionString));
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(Inbox);
        });
        builder.ConfigureLogging(logging => logging.ClearProviders());
    }

    public async Task InitializeDatabase()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    public async Task DropTestDatabase()
    {
        // Guard against ever targeting the configured application database.
        if (!DatabaseName.StartsWith("LibraryApiTests_", StringComparison.Ordinal) ||
            new SqlConnectionStringBuilder(ConnectionString).InitialCatalog != DatabaseName)
            throw new InvalidOperationException("Unsafe test database cleanup target.");
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeletedAsync();
    }
}
