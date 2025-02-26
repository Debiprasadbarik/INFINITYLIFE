using System.Data;
using Microsoft.Data.SqlClient;
using InfinityLife.Models;

namespace InfinityLife.DataAccess
{
    public class TimeSheetRepository
    {
        private readonly string _connectionString;

        public TimeSheetRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateTimeSheet(TimeSheet timeSheet)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO TimeSheet (EmployeeId, StartDate, EndDate, TotalHours, Status) " +
                    "VALUES (@EmployeeId, @StartDate, @EndDate, @TotalHours, @Status); SELECT SCOPE_IDENTITY()", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", timeSheet.EmployeeId);
                    cmd.Parameters.AddWithValue("@StartDate", timeSheet.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", timeSheet.EndDate);
                    cmd.Parameters.AddWithValue("@TotalHours", timeSheet.TotalHours);
                    cmd.Parameters.AddWithValue("@Status", "Pending");

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task CreateTimeSheetEntry(TimeSheetEntry entry)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO TimeSheetEntry (TimeSheetId, EntryDate, EntryType, ProjectName, " +
                    "LearningName, StatusMessage, Hours) VALUES (@TimeSheetId, @EntryDate, @EntryType, @ProjectName, @LearningName, " +
                    "@StatusMessage, @Hours)", conn))
                {
                    cmd.Parameters.AddWithValue("@TimeSheetId", entry.TimeSheetId);
                    cmd.Parameters.AddWithValue("@EntryDate", entry.EntryDate);
                    cmd.Parameters.AddWithValue("@EntryType", entry.EntryType);
                    cmd.Parameters.AddWithValue("@ProjectName", (object)entry.ProjectName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LearningName", (object)entry.LearningName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@StatusMessage", entry.StatusMessage);
                    cmd.Parameters.AddWithValue("@Hours", entry.Hours);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<TimeSheet>> GetTimeSheetsByDateRange(DateTime startDate, DateTime endDate, string employeeId = null)
        {
            var timeSheets = new List<TimeSheet>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "SELECT t.*, e.* FROM TimeSheet t " +
                            "INNER JOIN TimeSheetEntry e ON t.TimeSheetId = e.TimeSheetId " +
                            "WHERE t.StartDate >= @StartDate AND t.EndDate <= @EndDate";

                if (!string.IsNullOrEmpty(employeeId))
                {
                    sql += " AND t.EmployeeId = @EmployeeId";
                }

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Map the data to TimeSheet and TimeSheetEntry objects
                            // Add to the timeSheets list
                            // Implementation details omitted for brevity
                        }
                    }
                }
            }
            return timeSheets;
        }
        public async Task<TimeSheet> GetTimeSheetById(int id)
        {
            return null;
            // Implementation to retrieve a timesheet by ID with its entries
        }

        public async Task UpdateTimeSheetStatus(int id, string status, string reasonIfRejected = null)
        {
            // Implementation to update a timesheet's status
        }
    }
}