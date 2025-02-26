using Microsoft.Data.SqlClient;
using InfinityLife.DataAccess.Interfaces;
using InfinityLife.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using System.Data.SqlClient;
//using Dapper;
namespace InfinityLife.DataAccess.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string? _connectionString;
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(IConfiguration configuration, ILogger<EmployeeRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("DefaultConnection string is missing");
            _logger = logger;
        }

        public async Task<Employee> GetEmployeeByEmail(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string sql = @"
                SELECT e.*, r.EmpRole 
                FROM Employee e
                JOIN EmployeeRole r ON e.EmpRoleId = r.EmpRoleId
                WHERE e.EmpEmail = @Email";
                using var cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Email", email);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Employee
                    {
                        EmpId = reader.GetString(reader.GetOrdinal("EmpId")),
                        EmpEmail = reader.GetString(reader.GetOrdinal("EmpEmail")),
                        EmpPhone = reader["EmpPhone"]?.ToString(),
                        EmpAddress = reader["EmpAddress"]?.ToString(),
                        EmergencyContact = reader["EmergencyContact"]?.ToString(),
                        Bloodgroup = reader["Bloodgroup"]?.ToString(),
                        AadharNo = reader["AadharNo"]?.ToString(),
                        PanNo = reader["PanNo"]?.ToString(),
                        DateOfBirth = reader["DateOfBirth"] != DBNull.Value ?
                            (reader["DateOfBirth"] is DateTime ?
                                (DateTime)reader["DateOfBirth"] :
                                Convert.ToDateTime(reader["DateOfBirth"].ToString())) :
                            DateTime.MinValue,
                        DateOfJoining = reader["DateOfJoining"] != DBNull.Value ?
                            (reader["DateOfJoining"] is DateTime ?
                                (DateTime)reader["DateOfJoining"] :
                                Convert.ToDateTime(reader["DateOfJoining"].ToString())) :
                            DateTime.MinValue,
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        EmpFirstName = reader.GetString(reader.GetOrdinal("EmpFirstName")),
                        EmpLastName = reader.GetString(reader.GetOrdinal("EmpLastName")),
                        BasicSalary = reader["BasicSalary"] != DBNull.Value ? Convert.ToInt32(reader["BasicSalary"]) : 0,
                        HRA = reader["HRA"] != DBNull.Value ? Convert.ToInt32(reader["HRA"]) : 0,
                        Conveyance = reader["Conveyance"] != DBNull.Value ? Convert.ToInt32(reader["Conveyance"]) : 0,
                        OtherAllowances = reader["OtherAllowances"] != DBNull.Value ? Convert.ToInt32(reader["OtherAllowances"]) : 0,
                        PF = reader["PF"] != DBNull.Value ? Convert.ToInt32(reader["PF"]) : 0,
                        ESIC = reader["ESIC"] != DBNull.Value ? Convert.ToInt32(reader["ESIC"]) : 0,
                        ProfessionalTax = reader["ProfessionalTax"] != DBNull.Value ? Convert.ToInt32(reader["ProfessionalTax"]) : 0,
                        IncomeTax = reader["IncomeTax"] != DBNull.Value ? Convert.ToInt32(reader["IncomeTax"]) : 0,
                        EmployeeRole = new EmployeeRole
                        {
                            EmpRole = reader.GetString(reader.GetOrdinal("EmpRole"))
                        }
                        // Map other properties
                    };
                }
            }
            return null;
        }
        public async Task<List<EmployeeRole>> GetAllRoles()
        {
            try
            {
                var roles = new List<EmployeeRole>();
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT EmpRoleId, EmpRole FROM EmployeeRole "; //WHERE IsActive = 1
                using var command = new SqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    roles.Add(new EmployeeRole
                    {
                        EmpRoleId = reader.GetInt32(reader.GetOrdinal("EmpRoleId")),
                        EmpRole = reader.GetString(reader.GetOrdinal("EmpRole"))
                    });
                }

                return roles;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error retrieving employee roles");
                throw;
            }
        }
        public async Task<IEnumerable<Employee>> GetAllEmployees()
        {
            var employees = new List<Employee>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(@"
            SELECT e.*, r.EmpRole 
            FROM Employee e
            LEFT JOIN EmployeeRole r ON e.EmpRoleId = r.EmpRoleId", connection);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var employee = new Employee
                    {
                        EmpId = reader["EmpId"].ToString(),
                        EmpNo = reader["EmpNo"] != DBNull.Value? Convert.ToInt32(reader["EmpNo"]):0,
                        EmpRoleId = reader["EmpRoleId"] != DBNull.Value ? Convert.ToInt32(reader["EmpRoleId"]) : 0,
                        EmpFirstName = reader["EmpFirstName"]?.ToString(),
                        EmpLastName = reader["EmpLastName"]?.ToString(),
                        EmpPhone = reader["EmpPhone"]?.ToString(),
                        EmpEmail = reader["EmpEmail"]?.ToString(),
                        EmpAddress = reader["EmpAddress"]?.ToString(),
                        EmergencyContact = reader["EmergencyContact"]?.ToString(),
                        Bloodgroup =  reader["Bloodgroup"]?.ToString(),
                        AadharNo = reader["AadharNo"]?.ToString(),
                        PanNo = reader["PanNo"]?.ToString(),
                        DateOfBirth = reader["DateOfBirth"] != DBNull.Value ?
                            (reader["DateOfBirth"] is DateTime ?
                                (DateTime)reader["DateOfBirth"] :
                                Convert.ToDateTime(reader["DateOfBirth"].ToString())) :
                            DateTime.MinValue,
                        DateOfJoining = reader["DateOfJoining"] != DBNull.Value ?
                            (reader["DateOfJoining"] is DateTime ?
                                (DateTime)reader["DateOfJoining"] :
                                Convert.ToDateTime(reader["DateOfJoining"].ToString())) :
                            DateTime.MinValue,
                        Password = reader["Password"]?.ToString(),
                        BasicSalary = reader["BasicSalary"] != DBNull.Value ? Convert.ToInt32(reader["BasicSalary"]) : 0,
                        HRA = reader["HRA"] != DBNull.Value ? Convert.ToInt32(reader["HRA"]) : 0,
                        Conveyance = reader["Conveyance"] != DBNull.Value ? Convert.ToInt32(reader["Conveyance"]) : 0,
                        OtherAllowances = reader["OtherAllowances"] != DBNull.Value ? Convert.ToInt32(reader["OtherAllowances"]) : 0,
                        PF = reader["PF"] != DBNull.Value ? Convert.ToInt32(reader["PF"]) : 0,
                        ESIC = reader["ESIC"] != DBNull.Value ? Convert.ToInt32(reader["ESIC"]) : 0,
                        ProfessionalTax = reader["ProfessionalTax"] != DBNull.Value ? Convert.ToInt32(reader["ProfessionalTax"]) : 0,
                        IncomeTax = reader["IncomeTax"] != DBNull.Value ? Convert.ToInt32(reader["IncomeTax"]) : 0
                    };
                    if (!reader.IsDBNull(reader.GetOrdinal("EmpRole")))
                    {
                        employee.EmployeeRole = new EmployeeRole
                        {
                            EmpRoleId = employee.EmpRoleId,
                            EmpRole = reader.GetString(reader.GetOrdinal("EmpRole"))
                        };
                    }

                    employees.Add(employee);
                }
                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all employees");
                throw;
            }
        }

        public async Task<Employee> SearchEmployee(string searchTerm)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT TOP 1 * FROM Employee 
                            WHERE (Email LIKE @SearchTerm 
                            OR Name LIKE @SearchTerm 
                            OR PhoneNumber LIKE @SearchTerm)
                            AND IsActive = 1";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapEmployeeFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Employee> CreateEmployee(Employee employee)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int empNo = await GenerateEmployeeNumber(connection);
                    Console.WriteLine($"Inserting employee with RoleId: {employee.EmpRoleId}");
                    var query = @"INSERT INTO Employee 
                            (EmpNo,EmpRoleId, EmpFirstName, EmpLastName, EmpPhone, EmpEmail, EmpAddress,EmergencyContact,Bloodgroup,AadharNo,PanNo,DateOfBirth,DateOfJoining,Password,
                            BasicSalary,HRA,Conveyance,OtherAllowances,PF,ESIC,ProfessionalTax,IncomeTax)
                            OUTPUT INSERTED.EmpId
                            VALUES 
                            (@EmpNo,@Role,@FirstName,@LastName,@Phone, @Email,@Address,@Emc,@Blood,@Aadhaar,@Pan,@DOB,@DOJ,@Password,
                            @BasicSalary,@HRA,@Conveyance,@OtherAllowances,@PF,@ESIC,@ProfessionalTax,@IncomeTax);
                            SELECT SCOPE_IDENTITY();";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EmpNo", empNo);
                    command.Parameters.AddWithValue("@Role", employee.EmpRoleId);
                    command.Parameters.AddWithValue("@FirstName", employee.EmpFirstName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LastName", employee.EmpLastName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", employee.EmpPhone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", employee.EmpEmail);
                    command.Parameters.AddWithValue("@Address", employee.EmpAddress ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Emc", employee.EmergencyContact ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Blood", employee.Bloodgroup ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Aadhaar", employee.AadharNo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Pan", employee.PanNo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DOB",employee.DateOfBirth ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DOJ",employee.DateOfJoining ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Password", employee.Password);
                    command.Parameters.AddWithValue("@BasicSalary", employee.BasicSalary);
                    command.Parameters.AddWithValue("@HRA", employee.HRA);
                    command.Parameters.AddWithValue("@Conveyance", employee.Conveyance);
                    command.Parameters.AddWithValue("@OtherAllowances", employee.OtherAllowances);
                    command.Parameters.AddWithValue("@PF", employee.PF);
                    command.Parameters.AddWithValue("@ESIC", employee.ESIC);
                    command.Parameters.AddWithValue("@ProfessionalTax", employee.ProfessionalTax);
                    command.Parameters.AddWithValue("@IncomeTax", employee.IncomeTax);
                    //command.Parameters.AddWithValue("@ProfilePicture", profilePicture != null ? (object)profilePicture : DBNull.Value);
                    //command.Parameters.AddWithValue("@HireDate", DateTime.UtcNow);
                    _logger.LogInformation("Employee creation starting for: {@Employee}", new
                    {
                        employee.EmpEmail,
                        employee.EmpRoleId,
                        employee.EmpFirstName,
                        employee.EmpLastName
                    });
                    _logger.LogInformation("Executing CreateEmployee query for email: {Email}", employee.EmpEmail);
                    var insertedId = await command.ExecuteScalarAsync();
                    if (insertedId != null)
                    {
                        employee.EmpId = insertedId.ToString();
                        employee.EmpNo = empNo;
                        _logger.LogInformation("Successfully created employee with ID: {EmpId} {EmpNo}", employee.EmpId,employee.EmpNo);
                        return employee;
                    }
                    _logger.LogWarning("Failed to create employee. No ID returned.");
                    return null;
                    //return employee;
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error creating employee: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<int> GenerateEmployeeNumber(SqlConnection connection)
        {
            var query = @"SELECT ISNULL(MAX(EmpNo), 0) + 1 FROM Employee";

            using var command = new SqlCommand(query, connection);
            var nextNumber = await command.ExecuteScalarAsync();
            return Convert.ToInt32(nextNumber);
        }
        public async Task<Employee> GetEmployeeById(string id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(@"
                SELECT e.*, r.EmpRole 
                FROM Employee e
                LEFT JOIN EmployeeRole r ON e.EmpRoleId = r.EmpRoleId
                WHERE e.EmpId = @Id", connection)) 
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            //return MapEmployeeFromReader(reader);
                            return new Employee
                            {
                                EmpId = reader.GetString(reader.GetOrdinal("EmpId")),
                                EmpFirstName = reader.GetString(reader.GetOrdinal("EmpFirstName")),
                                EmpLastName = reader.GetString(reader.GetOrdinal("EmpLastName")),
                                EmpEmail = reader.GetString(reader.GetOrdinal("EmpEmail")),
                                BasicSalary = reader.GetInt32(reader.GetOrdinal("BasicSalary")),
                                HRA = reader.GetInt32(reader.GetOrdinal("HRA")),
                                Conveyance = reader.GetInt32(reader.GetOrdinal("Conveyance")),
                                OtherAllowances = reader.GetInt32(reader.GetOrdinal("OtherAllowances")),
                                PF = reader.GetInt32(reader.GetOrdinal("PF")),
                                ESIC = reader.GetInt32(reader.GetOrdinal("ESIC")),
                                ProfessionalTax = reader.GetInt32(reader.GetOrdinal("ProfessionalTax")),
                                IncomeTax = reader.GetInt32(reader.GetOrdinal("IncomeTax"))
                            };
                        }
                    }
                }
            }
            return null;
        }
        
        public async Task<bool> UpdateEmployee(Employee employee)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"UPDATE Employee
                    SET EmpFirstName = @FirstName, 
                        EmpLastName = @LastName,
                        EmpPhone = @Phone,
                        EmpEmail = @Mail,
                        EmpAddress = @Address,
                        Bloodgroup = @Bloodgroup,
                        AadharNo = @AadharNo,
                        PanNo = @PanNo,
                        DateOfBirth = @DOB,
                        DateOfJoining = @DOJ,
                        BasicSalary = @BasicSalary,
                        HRA = @HRA,
                        Conveyance = @Conveyance,
                        OtherAllowances = @OtherAllowances,
                        PF = @PF,
                        ESIC = @ESIC,
                        ProfessionalTax = @ProfessionalTax,
                        IncomeTax = @IncomeTax
                    WHERE EmpId = @EmpId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", employee.EmpFirstName);
                    command.Parameters.AddWithValue("@LastName", employee.EmpLastName);
                    command.Parameters.AddWithValue("@Phone", employee.EmpPhone);
                    command.Parameters.AddWithValue("@Mail", employee.EmpEmail);
                    command.Parameters.AddWithValue("@Address", employee.EmpAddress);
                    command.Parameters.AddWithValue("@Bloodgroup", employee.Bloodgroup);
                    command.Parameters.AddWithValue("@AadharNo", employee.AadharNo);
                    command.Parameters.AddWithValue("@PanNo", employee.PanNo);
                    command.Parameters.AddWithValue("@DOB",employee.DateOfBirth);
                    command.Parameters.AddWithValue("@DOJ",employee.DateOfJoining);
                    command.Parameters.AddWithValue("@BasicSalary", employee.BasicSalary);
                    command.Parameters.AddWithValue("@HRA", employee.HRA);
                    command.Parameters.AddWithValue("@Conveyance", employee.Conveyance);
                    command.Parameters.AddWithValue("@OtherAllowances", employee.OtherAllowances);
                    command.Parameters.AddWithValue("@PF", employee.PF);
                    command.Parameters.AddWithValue("@ESIC", employee.ESIC);
                    command.Parameters.AddWithValue("@ProfessionalTax", employee.ProfessionalTax);
                    command.Parameters.AddWithValue("@IncomeTax", employee.IncomeTax);
                    command.Parameters.AddWithValue("@EmpId", employee.EmpId);

                    await connection.OpenAsync();
                    var rowaffected= await command.ExecuteNonQueryAsync();
                    return rowaffected > 0;
                }
            }
        }

        public async Task<bool> DeleteEmployee(string id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    const string sql = "DELETE FROM Employee WHERE EmpId = @Id";

                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@Id", id);

                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    _logger.LogInformation("Employee deletion attempted. ID: {Id}, Rows affected: {RowsAffected}",
                        id, rowsAffected);

                    return rowsAffected > 0;
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while deleting employee {Id}: {Message}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        private Employee MapEmployeeFromReader(SqlDataReader reader)
        {
            var employee = new Employee
            {
                EmpId = reader["EmpId"]?.ToString(),
                EmpRoleId = Convert.ToInt32(reader["EmpRoleId"]),
                EmpFirstName = reader["EmpFirstName"]?.ToString(),
                EmpLastName = reader["EmpLastName"]?.ToString(),
                EmpPhone = reader["EmpPhone"]?.ToString(),
                EmpEmail = reader["EmpEmail"]?.ToString(),
                EmpAddress = reader["EmpAddress"]?.ToString(),
                EmergencyContact = reader["EmergencyContact"]?.ToString(),
                Bloodgroup = reader["Bloodgroup"]?.ToString(),
                AadharNo = reader["AadharNo"]?.ToString(),
                PanNo = reader["PanNo"]?.ToString(),
                DateOfBirth = reader["DateOfBirth"] != DBNull.Value ?
                    (reader["DateOfBirth"] is DateTime ?
                        (DateTime)reader["DateOfBirth"] :
                        Convert.ToDateTime(reader["DateOfBirth"].ToString())) :
                    DateTime.MinValue,
                DateOfJoining = reader["DateOfJoining"] != DBNull.Value ?
                    (reader["DateOfJoining"] is DateTime ?
                        (DateTime)reader["DateOfJoining"] :
                        Convert.ToDateTime(reader["DateOfJoining"].ToString())) :
                    DateTime.MinValue,
                Password = reader["Password"]?.ToString(),
                BasicSalary = Convert.ToInt32(reader["BasicSalary"]),
                HRA = Convert.ToInt32(reader["HRA"]),
                Conveyance = Convert.ToInt32(reader["Conveyance"]), 
                OtherAllowances = Convert.ToInt32(reader["OtherAllowances"]), 
                PF = Convert.ToInt32(reader["PF"]),
                ESIC = Convert.ToInt32(reader["ESIC"]),
                ProfessionalTax = Convert.ToInt32(reader["ProfessionalTax"]), 
                IncomeTax = Convert.ToInt32(reader["IncomeTax"]) 
                //ProfilePicture = reader["ProfilePicture"] as byte[],
                //HireDate = Convert.ToDateTime(reader["HireDate"]),
                //IsActive = Convert.ToBoolean(reader["IsActive"])
            };
            if (reader.HasRows && !reader.IsDBNull(reader.GetOrdinal("EmpRole")))
            {
                employee.EmployeeRole = new EmployeeRole
                {
                    EmpRoleId = employee.EmpRoleId,
                    EmpRole = reader.GetString(reader.GetOrdinal("EmpRole"))
                };
            }

            return employee;
        }
        public async Task UpdatePassword(string email, string newPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string sql = "UPDATE Employee SET Password = @NewPassword WHERE EmpEmail = @Email";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@NewPassword", newPassword);
                command.Parameters.AddWithValue("@Email", email);

                await command.ExecuteNonQueryAsync();
            }
        }
        public async Task UpdateEmployeeProfilePic(Employee employee, byte[] profilePicture)
        {
            ArgumentNullException.ThrowIfNull(profilePicture);
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"UPDATE Employee
                    SET ProfilePicture = @ProfilePicture WHERE EmpId = @EmpId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProfilePicture", profilePicture != null ? (object)profilePicture : DBNull.Value);
                    command.Parameters.AddWithValue("@EmpId", employee.EmpId);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
