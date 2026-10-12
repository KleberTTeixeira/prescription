using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Prescription.Application;
using Prescription.Data;
using Prescription.Data.Context;

namespace Prescription.IntegrationTests;

/// <summary>
/// Base of the integration tests: real Application + Data against the MySQL given in PRESCRIPTION_TEST_DB.
/// Each test runs inside a transaction that is rolled back at the end (the seed stays intact).
/// Without the variable the tests are skipped.
/// </summary>
public abstract class DatabaseTest : IAsyncLifetime
{
    public const string ConnectionVariable = "PRESCRIPTION_TEST_DB";

    private static readonly string? ConnectionString = Environment.GetEnvironmentVariable(ConnectionVariable);

    private ServiceProvider? _provider;
    private AsyncServiceScope _scope;
    private IDbContextTransaction? _transaction;

    protected IServiceProvider Services => _scope.ServiceProvider;

    protected PrescriptionDbContext Context => Services.GetRequiredService<PrescriptionDbContext>();

    protected T Get<T>() where T : notnull => Services.GetRequiredService<T>();

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            return;

        _provider = new ServiceCollection().AddData(ConnectionString).AddApplication().BuildServiceProvider();
        _scope = _provider.CreateAsyncScope();
        _transaction = await Context.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        if (_provider is not null)
        {
            await _scope.DisposeAsync();
            await _provider.DisposeAsync();
        }
    }

    /// <summary>Call at the start of each test: skips it when PRESCRIPTION_TEST_DB is not set.</summary>
    protected static void RequireDatabase() =>
        Skip.If(string.IsNullOrWhiteSpace(ConnectionString), $"Variável {ConnectionVariable} não definida.");
}
