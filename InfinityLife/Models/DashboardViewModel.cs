using InfinityLife.Models;
using System.Collections.Generic;

namespace InfinityLife.Models
{
    public class DashboardViewModel
    {
        public string? UserRole { get; set; }
        public Employee Employee { get; set; }
        public List<Leave> LeaveHistory { get; set; }
        public LeaveBalance leaveBalanceForEmployee { get; set; }
        public List<LeaveBalance> LeaveBalances{ get; set; }
        public List<CalendarHoliday> Holidays { get; set; }
        public List<Leave> PendingLeaveRequests { get; set; }
        public byte[]? ProfilePicture { get; set; }
        // Statistics for Director Dashboard
        public int TotalEmployees { get; set; }
        public int Departments { get; set; }
        public Dictionary<string, int> DepartmentDistribution { get; set; }
        public Dictionary<string, int> EmployeeRoleDistribution { get; set; }
        public IEnumerable<Employee> AllEmployees { get; set; }
        public IEnumerable<PaySlip> PaySlips { get; set; }
        // Statistics for HR Dashboard
        public List<Employee>? RecentEmployees { get; set; }
        public int ActiveEmployees { get; set; }

        // Statistics for Accountant Dashboard
        public decimal TotalPayroll { get; set; }
        public int PendingSalaryReviews { get; set; }

        // Common Dashboard Statistics
        public int NotificationCount { get; set; }
        public List<string>? RecentNotifications { get; set; }
        public Dictionary<string, bool>? UserPermissions { get; set; }
        public IEnumerable<PaySlip>? UnreadPaySlips { get; set; }
        public class DebugInformation
        {
            public string UserEmail { get; set; }
            public string UserRole { get; set; }
            public string EmployeeId { get; set; }
            public bool HasPaySlips { get; set; }
            public int PaySlipsCount { get; set; }
        }
        public DashboardViewModel()
        {
            // Initialize collections to prevent null reference exceptions
            PaySlips = new List<PaySlip>();
            RecentEmployees = new List<Employee>();
            RecentNotifications = new List<string>();
            UserPermissions = new Dictionary<string, bool>();
            PendingLeaveRequests = new List<Leave>();
            LeaveHistory = new List<Leave>();
            LeaveBalances = new List<LeaveBalance>() { };
            leaveBalanceForEmployee = new LeaveBalance();
            AllEmployees = new List<Employee>();
        }
    }
}