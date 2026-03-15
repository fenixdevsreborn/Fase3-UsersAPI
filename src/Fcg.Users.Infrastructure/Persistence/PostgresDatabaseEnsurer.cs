using Npgsql;

namespace Fcg.Users.Infrastructure.Persistence;

/// <summary>Ensures the PostgreSQL database from the connection string exists before running migrations.</summary>
public static class PostgresDatabaseEnsurer
{
    /// <summary>Creates the database if it does not exist. Use before MigrateAsync() when the database might not exist yet.</summary>
    public static async Task EnsureExistsAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
            return;

        builder.Database = "postgres";
        var masterConnectionString = builder.ToString();

        await using var connection = new NpgsqlConnection(masterConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var exists = await ExistsAsync(connection, databaseName, cancellationToken).ConfigureAwait(false);
        if (!exists)
        {
            await CreateDatabaseAsync(connection, databaseName, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<bool> ExistsAsync(NpgsqlConnection connection, string databaseName, CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
        cmd.Parameters.AddWithValue("name", databaseName);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result != null;
    }

    private static async Task CreateDatabaseAsync(NpgsqlConnection connection, string databaseName, CancellationToken cancellationToken)
    {
        var quoted = QuoteIdentifier(databaseName);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"CREATE DATABASE {quoted}";
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string QuoteIdentifier(string name)
    {
        return "\"" + name.Replace("\"", "\"\"") + "\"";
    }
}
