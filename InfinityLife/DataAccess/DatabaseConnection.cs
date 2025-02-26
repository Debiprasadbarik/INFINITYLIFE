using Microsoft.Data.SqlClient;

public class DatabaseConnection
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseConnection> _logger;

    public DatabaseConnection(IConfiguration configuration, ILogger<DatabaseConnection> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    public async Task<SqlConnection> CreateConnectionAsync()
    {
        try
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to create database connection");
            throw new ApplicationException("Failed to connect to the database. Please contact support.", ex);
        }
    }

    public SqlConnection CreateConnection()
    {
        try
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to create database connection");
            throw new ApplicationException("Failed to connect to the database. Please contact support.", ex);
        }
    }
}