using InfinityLife.DataAccess.Interfaces;
using InfinityLife.Models;
using Microsoft.Data.SqlClient;

namespace InfinityLife.DataAccess.Repositories
{
    public class EmployeeRoleRepository : IEmployeeRoleRepository
    {
        private readonly DatabaseConnection? _db;
        private readonly ILogger<EmployeeRoleRepository>? _logger;
        private readonly string? _connectionString;

        public EmployeeRoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public EmployeeRoleRepository(DatabaseConnection db, ILogger<EmployeeRoleRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeRole>> GetAllRoles()
        {
            var roles = new List<EmployeeRole>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("SELECT * FROM EmployeeRole", connection))
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            roles.Add(new EmployeeRole
                            {
                                EmpRoleId = Convert.ToInt32(reader["EmpRoleId"]),
                                EmpRole = reader["EmpRole"].ToString()
                            });
                        }
                    }
                }
            }
            return roles;
        }

        public async Task<EmployeeRole> GetRoleById(int id)
        {
            try
            {
                using var conn = await _db.CreateConnectionAsync();
                const string sql = "SELECT EmpRoleId, EmpRole FROM EmployeeRole WHERE EmpRoleId = @RoleId";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RoleId", id);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new EmployeeRole
                    {
                        EmpRoleId = reader.GetInt32(reader.GetOrdinal("EmpRoleId")),
                        EmpRole = reader.GetString(reader.GetOrdinal("EmpRole"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee role by id: {Id}", id);
                throw;
            }
        }
    }
}