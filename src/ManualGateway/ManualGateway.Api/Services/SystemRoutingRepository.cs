using Dapper;
using ManualGateway.Api.Models;
using Npgsql;

namespace ManualGateway.Api.Services;

public class SystemRoutingRepository
{
    private readonly string _connectionString;

    // We'll get the connection string from IConfiguration (appsettings.json)
    public SystemRoutingRepository(IConfiguration configuration)
    {
        // Connection string for our Dockerized PostgreSQL
        // Assumes docker-compose service name 'postgres-db' and credentials from docker-compose.yml
        // When Gateway.Api runs in Docker, 'postgres-db' resolves to the DB container's IP.
        _connectionString = configuration.GetConnectionString("PostgresConnection")
                            ?? throw new InvalidOperationException("PostgresConnection not found in configuration.");
    }

    public async Task<SystemRoute?> GetRouteBySystemIdAsync(string systemId)
    {
        const string query = "SELECT SystemId, ProductServiceTarget " +
                             "FROM SystemProductRoutes" +
                             "WHERE SystemId = @SystemIdParam;";
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<SystemRoute>(query, new { SystemIdParam = systemId });
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Database query failed: {ex.Message}"); // Simple console log for now
            return null; // Or rethrow, or handle as appropriate
        }
    }

    public async Task<IEnumerable<SystemRoute>> GetAllRoutesAsync()
    {
        const string query = "SELECT SystemId, ProductServiceTarget " +
                             "FROM SystemProductRoutes;";
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<SystemRoute>(query);
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Database query failed: {ex.Message}");
            return Enumerable.Empty<SystemRoute>();
        }
    }
}