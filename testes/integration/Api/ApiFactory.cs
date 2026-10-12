using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySql.Data.MySqlClient;
using Prescription.Data.Context;

namespace Prescription.IntegrationTests.Api;

/// <summary>
/// Starts the real API in memory against MySQL. With shareTransaction (default) all requests share one
/// connection with a transaction that is rolled back on dispose, so the seed stays intact. Without it the
/// API runs with its own services unchanged, using only the given connection string (read-only checks
/// such as /health).
/// </summary>
internal sealed class ApiFactory(string connectionString, bool shareTransaction = true) : WebApplicationFactory<Program>
{
    private readonly MySqlConnection _connection = new(connectionString);
    private MySqlTransaction? _transaction;
    private bool _rolledBack;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:PrescriptionDb", connectionString);

        if (!shareTransaction)
            return;

        builder.ConfigureTestServices(services =>
        {
            _connection.Open();
            _transaction = _connection.BeginTransaction();

            var options = new DbContextOptionsBuilder<PrescriptionDbContext>().UseMySQL(_connection).Options;

            services.RemoveAll<DbContextOptions<PrescriptionDbContext>>();
            services.RemoveAll<PrescriptionDbContext>();
            services.AddScoped(_ =>
            {
                var context = new PrescriptionDbContext(options);
                context.Database.UseTransaction(_transaction);
                return context;
            });
        });
    }

    /// <summary>WebApplicationFactory calls Dispose(bool) more than once: the rollback must run only once.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_rolledBack)
        {
            _rolledBack = true;
            _transaction?.Rollback();
            _transaction?.Dispose();
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
