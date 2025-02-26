using InfinityLife.DataAccess.Interfaces;
using InfinityLife.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace InfinityLife.DataAccess.Repositories
{
    public class PaySlipRepository : IPaySlipRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<PaySlipRepository> _logger;

        public PaySlipRepository(IConfiguration configuration, ILogger<PaySlipRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("DefaultConnection string is missing");
            _logger = logger;
        }
        public async Task<bool> EmployeeExists(string employeeId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(1) FROM Employee WHERE EmpId = @EmployeeId";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeId", employeeId);

            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }
        public async Task<PaySlip> CreatePaySlip(PaySlip paySlip)
        {
            if (!await EmployeeExists(paySlip.EmployeeId))
            {
                throw new InvalidOperationException($"Employee with ID {paySlip.EmployeeId} does not exist.");
            }
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"INSERT INTO PaySlip 
                    (EmployeeId, BasicSalary,HRA,ConveyanceAllowance,OtherAllowance,Bonus,PF,ESIC,ProfessionalTax,IncomeTax,OtherDeductions,PaymentDate,CreatedAt,UpdatedBy)
                    OUTPUT INSERTED.PaySlipId
                    VALUES 
                    (@EmployeeId,@BasicSalary,@HRA ,@ConveyanceAllowance,@OtherAllowance,@Bonus,@PF,@ESIC,@ProfessionalTax,@IncomeTax,@OtherDeductions,@PaymentDate,
                     @CreatedAt,@UpdatedBy)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeId", paySlip.EmployeeId);
            command.Parameters.AddWithValue("@EmployeeId", paySlip.PayPeriod);
            command.Parameters.AddWithValue("@BasicSalary", paySlip.BasicSalary);
            command.Parameters.AddWithValue("@HRA", paySlip.HRA);
            command.Parameters.AddWithValue("@ConveyanceAllowance", paySlip.Conveyance);
            command.Parameters.AddWithValue("@OtherAllowance", paySlip.OtherAllowances);
            command.Parameters.AddWithValue("@Bonus", paySlip.Bonus);
            command.Parameters.AddWithValue("@PF", paySlip.PF);
            command.Parameters.AddWithValue("@ESIC", paySlip.ESIC);
            command.Parameters.AddWithValue("@ProfessionalTax", paySlip.ProfessionalTax);
            command.Parameters.AddWithValue("@IncomeTax", paySlip.IncomeTax);
            command.Parameters.AddWithValue("@IncomeTax", paySlip.GeneratedDate);
            paySlip.PaySlipId = (int)await command.ExecuteScalarAsync();
            return paySlip;
        }

        public async Task<PaySlip> GetPaySlipById(string employeeId, DateTime payperiod)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentException("Employee ID cannot be null or empty", nameof(employeeId));
            }
            using var connection = new SqlConnection(_connectionString);
            var query = @"SELECT 
                    e.EmpId as EmployeeId,
                    e.EmpFirstName,
                    e.EmpLastName,
                    e.EmpEmail,
                    e.BasicSalary,
                    e.HRA,
                    e.Conveyance as ConveyanceAllowance,
                    e.OtherAllowances,
                    e.PF,
                    e.ESIC,
                    e.ProfessionalTax,
                    e.IncomeTax
                FROM Employee e
                WHERE e.EmpId = @EmployeeId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeId", employeeId);
            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var paySlip = new PaySlip
                    {
                        EmployeeId = employeeId,
                        PayPeriod = payperiod,
                        GeneratedDate = DateTime.Now,
                        BasicSalary = reader.GetInt32(reader.GetOrdinal("BasicSalary")),
                        HRA = reader.GetInt32(reader.GetOrdinal("HRA")),
                        Conveyance = reader.GetInt32(reader.GetOrdinal("ConveyanceAllowance")),
                        OtherAllowances = reader.GetInt32(reader.GetOrdinal("OtherAllowances")),
                        PF = reader.GetInt32(reader.GetOrdinal("PF")),
                        ESIC = reader.GetInt32(reader.GetOrdinal("ESIC")),
                        ProfessionalTax = reader.GetInt32(reader.GetOrdinal("ProfessionalTax")),
                        IncomeTax = reader.GetInt32(reader.GetOrdinal("IncomeTax")),
                        Employee = new Employee
                        {
                            EmpId = employeeId,
                            EmpFirstName = reader.GetString(reader.GetOrdinal("EmpFirstName")),
                            EmpLastName = reader.GetString(reader.GetOrdinal("EmpLastName")),
                            EmpEmail = reader.GetString(reader.GetOrdinal("EmpEmail"))
                        }
                    };
                    return paySlip;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pay slip for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task<IEnumerable<PaySlip>> GetPaySlipsByEmployeeId(string employeeId)
        {
            //var paySlips = new List<PaySlip>();
           // try
           // {
            //    using (var connection = new SqlConnection(_connectionString))
            //    {
            //        await connection.OpenAsync();
            //        var query = @"SELECT * FROM PaySlip 
            //                 WHERE EmployeeId = @EmployeeId 
            //                 ORDER BY PayPeriod DESC";

            //        using (var command = new SqlCommand(query, connection))
            //        {
            //            command.Parameters.AddWithValue("@EmployeeId", employeeId);
            //            using (var reader = await command.ExecuteReaderAsync())
            //            {
            //                while (await reader.ReadAsync())
            //                {
            //                    paySlips.Add(new PaySlip
            //                    {
            //                        PaySlipId = reader.GetInt32(reader.GetOrdinal("PaySlipId")),
            //                        EmployeeId = reader.GetString(reader.GetOrdinal("EmployeeId")),
            //                        PayPeriod = reader.GetDateTime(reader.GetOrdinal("PayPeriod")),
            //                        BasicSalary = reader.GetInt32(reader.GetOrdinal("BasicSalary")),
            //                        HRA = reader.GetInt32(reader.GetOrdinal("HRA")),
            //                        Conveyance = reader.GetInt32(reader.GetOrdinal("Conveyance")),
            //                        OtherAllowances = reader.GetInt32(reader.GetOrdinal("OtherAllowances")),
            //                        PF = reader.GetInt32(reader.GetOrdinal("PF")),
            //                        ESIC = reader.GetInt32(reader.GetOrdinal("ESIC")),
            //                        ProfessionalTax = reader.GetInt32(reader.GetOrdinal("ProfessionalTax")),
            //                        IncomeTax = reader.GetInt32(reader.GetOrdinal("IncomeTax")),
            //                        GeneratedDate = reader.GetDateTime(reader.GetOrdinal("GeneratedDate"))
            //                    });
            //                }
            //            }
            //        }
                //}
            var currentPaySlip = await GetPaySlipById(employeeId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
            return currentPaySlip != null ? new[] { currentPaySlip } : Array.Empty<PaySlip>();

            //return paySlips;
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error retrieving pay slips for employee {EmployeeId}", employeeId);
            //    throw;
            //}

            
        }

        //public async Task<bool> MarkAsRead(int paySlipId)
        //{
        //    using var connection = new SqlConnection(_connectionString);
        //    var query = "UPDATE PaySlip SET IsRead = 1 WHERE PaySlipId = @PaySlipId";

        //    using var command = new SqlCommand(query, connection);
        //    command.Parameters.AddWithValue("@PaySlipId", paySlipId);

        //    await connection.OpenAsync();
        //    var rowsAffected = await command.ExecuteNonQueryAsync();
        //    return rowsAffected > 0;
        //}

        //private PaySlip MapPaySlipFromReader(SqlDataReader reader)
        //{
        //    return new PaySlip
        //    {
        //        EmployeeId = employeeId,
        //        PayPeriod = payPeriod,
        //        GeneratedDate = DateTime.Now,
        //        BasicSalary = reader.GetInt32(reader.GetOrdinal("BasicSalary")),
        //        HRA = reader.GetInt32(reader.GetOrdinal("HRA")),
        //        Conveyance = reader.GetInt32(reader.GetOrdinal("ConveyanceAllowance")),
        //        OtherAllowances = reader.GetInt32(reader.GetOrdinal("OtherAllowances")),
        //        PF = reader.GetInt32(reader.GetOrdinal("PF")),
        //        ESIC = reader.GetInt32(reader.GetOrdinal("ESIC")),
        //        ProfessionalTax = reader.GetInt32(reader.GetOrdinal("ProfessionalTax")),
        //        IncomeTax = reader.GetInt32(reader.GetOrdinal("IncomeTax")),
        //        Employee = new Employee
        //        {
        //            EmpId = employeeId,
        //            EmpFirstName = reader.GetString(reader.GetOrdinal("EmpFirstName")),
        //            EmpLastName = reader.GetString(reader.GetOrdinal("EmpLastName")),
        //            EmpEmail = reader.GetString(reader.GetOrdinal("EmpEmail"))
        //        }
        //    };
        //}
        
    }
}
