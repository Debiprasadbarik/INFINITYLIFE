// DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InfinityLife.Models;
using System.Security.Claims;
using InfinityLife.DataAccess.Interfaces;
using InfinityLife.DataAccess.Repositories;

namespace InfinityLife.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        //private readonly DatabaseConnection? _context;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPaySlipRepository _paySlipRepository;
        private readonly ILogger<DashboardController> _logger;
        private readonly LeaveManager _leaveManager;
        private readonly string? _connectionString;
        public DashboardController(IEmployeeRepository employeeRepository, IPaySlipRepository paySlipRepository, ILogger<DashboardController> logger, IConfiguration configuration)
        {
            _employeeRepository = employeeRepository;
            _paySlipRepository = paySlipRepository;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection"); ;
            _leaveManager = new LeaveManager(_connectionString);

        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Name);
                //_logger.LogInformation("Loading dashboard for user email: {Email}", userEmail);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                //_logger.LogInformation("User role: {Role}", userRole);
                var employeeId = User.FindFirstValue("EmployeeId");
                var employee = await _employeeRepository.GetEmployeeByEmail(userEmail);
                //_logger.LogInformation("Retrieved employee data: ID={EmpId}, Email={EmpEmail}, FirstName={FirstName}",
                //    employee?.EmpId,
                //    employee?.EmpEmail,
                //    employee?.EmpFirstName);

                if (employee == null)
                {
                    _logger.LogWarning("No employee found for email: {Email}", userEmail);
                    return RedirectToAction("Login", "Login");
                }
                //var properties = employee.GetType().GetProperties();
                //foreach (var prop in properties)
                //{
                //    _logger.LogInformation("Property {PropertyName} = {Value}",
                //        prop.Name,
                //        prop.GetValue(employee)?.ToString() ?? "null");
                //}

                _logger.LogInformation("Found employee with ID: {EmpId}", employee.EmpId);
                //var paySlips = await _paySlipRepository.GetPaySlipsByEmployeeId(employee.EmpId);
                var viewModel = new DashboardViewModel
                {
                    UserRole = userRole,
                    Employee = employee
                    //PaySlips = paySlips ?? new List<PaySlip>()
                };
               
                switch (userRole?.ToLower())
                {
                    case "director":
                        {
                            viewModel.AllEmployees = await _employeeRepository.GetAllEmployees();
                            var allLeaveBalances = await _leaveManager.GetAllLeaveBalances();
                            viewModel.LeaveBalances = allLeaveBalances;
                            return View("DirectorDashboard", viewModel);
                        }
                    case "hr":
                        //viewModel.RecentEmployees = await _employeeRepository.GetRecentEmployees(5);
                        return View("HRDashboard", viewModel);

                    case "accountant":
                        return View("AccountantDashboard", viewModel);

                    default:
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(employee.EmpId))
                                {
                                    var employeeLeaveBalance = await _leaveManager.GetLeaveBalance(employee.EmpId);
                                    viewModel.LeaveBalances = new List<LeaveBalance> { employeeLeaveBalance };
                                    _logger.LogInformation("Fetching pay slips for employee ID: {EmpId}", employee.EmpId);
                                    var paySlip = await _paySlipRepository.GetPaySlipsByEmployeeId(employee.EmpId);
                                    if (paySlip != null && paySlip.Any())
                                    {
                                        _logger.LogInformation("Found {Count} pay slips for employee", paySlip.Count());
                                        viewModel.PaySlips = paySlip;
                                    }
                                    else
                                    {
                                        _logger.LogInformation("No pay slips found for employee ID: {EmpId}", employee.EmpId);
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("Employee has no EmpId");
                                    _logger.LogWarning("Employee is null or has no EmpId");
                                    viewModel.PaySlips = new List<PaySlip>();
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error retrieving pay slips for employee {EmpId}", employee?.EmpId);
                                viewModel.PaySlips = new List<PaySlip>();
                            }
                            ViewBag.DebugInfo = new
                            {
                                UserEmail = userEmail,
                                UserRole = userRole,
                                EmployeeId = employee.EmpId,
                                HasPaySlips = viewModel.PaySlips?.Any() ?? false,
                            };
                            return View("EmployeeDashboard", viewModel);
                        }
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Login");
            }
        }
    }
}
