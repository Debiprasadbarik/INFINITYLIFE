using Dapper;
using InfinityLife.Models;
using Microsoft.Data.SqlClient;
using System.Data;
//using System.Data;

public class LeaveManager
{
    private readonly string _connectionString;

    public LeaveManager(string connectionString)
    {
        _connectionString = connectionString;
    }
    public async Task<List<Leave>> GetLeaveHistory(string employeeId)
    {
        List<Leave> leaves = new List<Leave>();
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT l.*
                FROM Leaves l
                JOIN Employee e ON l.EmployeeId = e.EmpId
                WHERE l.EmployeeId = @EmployeeId
                ORDER BY l.RequestDate DESC", conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        leaves.Add(new Leave
                        {
                            LeaveId = reader.GetInt32("LeaveId"),
                            EmployeeId = reader.GetString("EmployeeId"),
                            FromDate = reader.GetDateTime("FromDate"),
                            ToDate = reader.GetDateTime("ToDate"),
                            Reason = reader.GetString("Reason"),
                            Status = reader.GetString("Status"),
                            RequestDate = reader.GetDateTime("RequestDate"),
                            ResponseComment = !reader.IsDBNull(reader.GetOrdinal("ResponseComment"))
                            ? reader.GetString("ResponseComment")
                            : null
                        });
                    }
                }
            }
        }
        return leaves;
    }
    public async Task<List<LeaveBalance>> GetAllLeaveBalances()
    {
        List<LeaveBalance> balances = new List<LeaveBalance>();
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
            SELECT b.EmployeeId, e.EmpFirstName + ' ' + e.EmpLastName as EmployeeName, 
                   b.TotalLeaves, b.RemainingLeaves
            FROM LeaveBalances b
            JOIN Employee e ON b.EmployeeId = e.EmpId", conn))
            {
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        balances.Add(new LeaveBalance
                        {
                            EmployeeId = reader.GetString("EmployeeId"),
                            TotalLeaves = reader.GetInt32("TotalLeaves"),
                            RemainingLeaves = reader.GetInt32("RemainingLeaves")
                        });
                            
                    }
                }
            }
        }
        return balances;
    }

    public async Task<bool> RequestLeave(Leave leave)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Leaves (EmployeeId, FromDate, ToDate, Reason, Status, RequestDate)
                VALUES (@EmployeeId, @FromDate, @ToDate, @Reason, 'Pending', @RequestDate)", conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeId", leave.EmployeeId);
                cmd.Parameters.AddWithValue("@FromDate", leave.FromDate);
                cmd.Parameters.AddWithValue("@ToDate", leave.ToDate);
                cmd.Parameters.AddWithValue("@Reason", leave.Reason);
                cmd.Parameters.AddWithValue("@RequestDate", DateTime.Now);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }

    public async Task<List<Leave>> GetPendingLeaveRequests()
    {
        List<Leave> leaves = new List<Leave>();
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT l.LeaveId,
                   l.EmployeeId,
                   l.FromDate,
                   l.ToDate,
                   l.Reason,
                   l.Status,
                   l.RequestDate
            FROM Leaves l
            JOIN Employee e ON l.EmployeeId = e.EmpId
            WHERE l.Status = 'Pending'", conn))
            {
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        leaves.Add(new Leave
                        {
                            LeaveId = reader.GetInt32("LeaveId"),
                            EmployeeId = reader.GetString("EmployeeId"),
                            FromDate = reader.GetDateTime("FromDate"),
                            ToDate = reader.GetDateTime("ToDate"),
                            Reason = reader.GetString("Reason"),
                            Status = reader.GetString("Status"),
                            RequestDate = reader.GetDateTime("RequestDate")
                        });
                    }
                }
            }
        }
        return leaves;
    }

    public async Task<int> CalculateWorkingDays(DateTime fromDate, DateTime toDate)
    {
        int workingDays = 0;
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT HolidayDate FROM CalendarHolidays 
                WHERE HolidayDate BETWEEN @FromDate AND @ToDate", conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                List<DateTime> holidays = new List<DateTime>();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        holidays.Add(reader.GetDateTime(0));
                    }
                }

                for (DateTime date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek != DayOfWeek.Saturday &&
                        date.DayOfWeek != DayOfWeek.Sunday &&
                        !holidays.Contains(date))
                    {
                        workingDays++;
                    }
                }
            }
        }
        return workingDays;
    }

    public async Task<bool> UpdateLeaveStatus(int leaveId, string status, string comment)
    {
        if (string.IsNullOrEmpty(status))
        {
            Console.WriteLine("status is not supplied");
        }
        if (string.IsNullOrEmpty(comment))
        {
            Console.WriteLine("Comment is empty");
        }
        if (leaveId == null || leaveId==0)
        {
            Console.WriteLine("LeaveId empty");
        }
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Leaves 
                SET Status = @Status, ResponseComment = @Comment 
                WHERE LeaveId = @LeaveId", conn))
            {
                cmd.Parameters.AddWithValue("@LeaveId", leaveId);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Comment", comment);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }

    public async Task<LeaveBalance> GetLeaveBalance(string employeeId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // First try to get existing balance
            var balance = await connection.QueryFirstOrDefaultAsync<LeaveBalance>(@"
            SELECT * FROM LeaveBalances WHERE EmployeeId = @EmployeeId AND Year = @CurrentYear",
                new
                {
                    EmployeeId = employeeId,
                    CurrentYear = DateTime.Now.Year
                });

            // If no balance exists, create a new entry with default balance
            if (balance == null)
            {
                const int defaultBalance = 10; // Default annual leave balance

                // Insert new balance record
                var insertQuery = @"
                INSERT INTO LeaveBalances (EmployeeId, TotalLeaves, RemainingLeaves, Year)
                VALUES (@EmployeeId, @DefaultBalance, @DefaultBalance, @CurrentYear);
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = await connection.ExecuteScalarAsync<int>(insertQuery, new
                {
                    EmployeeId = employeeId,
                    DefaultBalance = defaultBalance,
                    CurrentYear = DateTime.Now.Year
                });

                // Return the newly created balance
                balance = new LeaveBalance
                {
                    Id = id,
                    EmployeeId = employeeId,
                    TotalLeaves = defaultBalance,
                    RemainingLeaves = defaultBalance,
                    Year = DateTime.Now.Year
                };
            }

            return balance;
        }
    }

    // Add these methods inside your existing LeaveManager class
    public async Task<(string FullName, string Email)> GetEmployeeDetails(string employeeId)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
            SELECT EmpFirstName + ' ' + EmpLastName as FullName, EmpEmail
            FROM Employee 
            WHERE EmpId = @EmployeeId", conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return (
                            reader.GetString("FullName"),
                            reader.GetString("EmpEmail")
                        );
                    }
                    throw new Exception($"Employee not found: {employeeId}");
                }
            }
        }
    }

    public async Task<Leave> GetLeaveById(int leaveId)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
            SELECT * FROM Leaves 
            WHERE LeaveId = @LeaveId", conn))
            {
                cmd.Parameters.AddWithValue("@LeaveId", leaveId);
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Leave
                        {
                            LeaveId = reader.GetInt32("LeaveId"),
                            EmployeeId = reader.GetString("EmployeeId"),
                            FromDate = reader.GetDateTime("FromDate"),
                            ToDate = reader.GetDateTime("ToDate"),
                            Reason = reader.GetString("Reason"),
                            Status = reader.GetString("Status"),
                            RequestDate = reader.GetDateTime("RequestDate"),
                            ResponseComment = !reader.IsDBNull(reader.GetOrdinal("ResponseComment"))
                                ? reader.GetString("ResponseComment")
                                : null
                        };
                    }
                    throw new Exception($"Leave request not found: {leaveId}");
                }
            }
        }
    }
}
