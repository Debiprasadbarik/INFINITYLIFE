using InfinityLife.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
using Dapper;
using InfinityLife.Services;  // Make sure to add this using statement
public class LeaveController : Controller
{
    private readonly LeaveManager _leaveManager;
    private readonly EmailService _emailService;
    private readonly string? _connectionString;

    public LeaveController(IConfiguration configuration)
    {
        _connectionString = _connectionString = configuration.GetConnectionString("DefaultConnection"); ;
        _leaveManager = new LeaveManager(_connectionString);
        _emailService = new EmailService(configuration);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string employeeId)
    {
        var pendingRequests = await _leaveManager.GetPendingLeaveRequests();
        var leaveBalances = await _leaveManager.GetAllLeaveBalances();
        var viewModel = new DashboardViewModel
        {
            PendingLeaveRequests = pendingRequests,
            LeaveBalances = leaveBalances
        };

        return View(viewModel);
        //return View(new { PendingLeaveRequests = pendingRequests, LeaveBalances = leaveBalances });
        //var leaveHistory = await _leaveManager.GetLeaveHistory(employeeId);
        //var leaveBalance = await _leaveManager.GetLeaveBalance(employeeId);
        //return View(new { LeaveHistory = leaveHistory, LeaveBalance = leaveBalance });
    }
    [HttpGet]
    public IActionResult Request()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Request(Leave leave)
    {
            if (ModelState.IsValid)
            {
                try
                {
                    // Calculate working days excluding holidays
                    int workingDays = await _leaveManager.CalculateWorkingDays(leave.FromDate, leave.ToDate);
                    Console.WriteLine($"Calculated working days: {workingDays}");
                // Get employee's leave balance
                    var balance = await _leaveManager.GetLeaveBalance(leave.EmployeeId);
                    if (balance == null)
                    {
                        return Json(new { success = false, message = $"Unable to find leave balance for employee {leave.EmployeeId}" });
                    }
                    Console.WriteLine($"Leave balance found: {balance.RemainingLeaves} days remaining");
                    Console.WriteLine($"Processing leave request for employee {leave.EmployeeId} " +
                     $"from {leave.FromDate:yyyy-MM-dd} to {leave.ToDate:yyyy-MM-dd}");
                    if (leave.FromDate > leave.ToDate)
                    {
                        ModelState.AddModelError("", "Start date must be before or equal to end date");
                        return Json(new { success = false, message = "Start date must be before or equal to end date" });
                    }

                    if (leave.FromDate < DateTime.Today)
                    {
                        ModelState.AddModelError("", "Cannot request leave for past dates");
                        return Json(new { success = false, message = "Cannot request leave for past dates" });
                    }

                    if (workingDays > balance.RemainingLeaves)
                    {
                        ModelState.AddModelError("", "Insufficient leave balance");
                        return Json(new { success = false, message = "Insufficient leave balance" });
                        //return View(leave);
                    }

                    if (await _leaveManager.RequestLeave(leave))
                    {
                        var employeeDetails = await _leaveManager.GetEmployeeDetails(leave.EmployeeId);

                        // Send email to director
                        await _emailService.SendLeaveRequestEmail(
                            leave,
                            employeeDetails.FullName,
                            employeeDetails.Email
                        );
                        TempData["Success"] = "Leave request submitted successfully";
                        return Json(new { success = true });
                        //return RedirectToAction("Index", "Employee");
                    }
                    return Json(new { success = false, message = "Failed to submit leave request" });
                }
                catch (Exception ) 
                {
                    return Json(new { success = false, message = "An error occurred while processing your request" });
                }
            }
            return Json(new
            {
                success = false,
                message = string.Join("; ", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage))
            });
        //return View(leave);
    }

    //[HttpGet]
    //public async Task<IActionResult> Director()
    //{
    //    var pendingRequests = await _leaveManager.GetPendingLeaveRequests();
    //    var leaveBalances = await _leaveManager.GetAllLeaveBalances();
    //    return View(new { PendingLeaveRequests = pendingRequests, LeaveBalances = leaveBalances });
    //}
    [HttpPost]
    public async Task<IActionResult> Respond(int leaveId, string status, string comment)
    {
        comment = string.IsNullOrWhiteSpace(comment)? (status == "Approved" ? "Ok" : "No") : comment;
        if (await _leaveManager.UpdateLeaveStatus(leaveId, status, comment))
        {
            var leave = await _leaveManager.GetLeaveById(leaveId);
            var employeeDetails = await _leaveManager.GetEmployeeDetails(leave.EmployeeId);
            // Send email to employee
            await _emailService.SendLeaveStatusUpdateEmail(
                leave,
                employeeDetails.FullName,
                employeeDetails.Email
            );
            return RedirectToAction("Index");
        }
        else
        {
            return RedirectToAction("Error");
        }
    }
    [HttpGet]
    public async Task<LeaveBalance> GetLeaveBalance(string employeeId)
    {
        return await _leaveManager.GetLeaveBalance(employeeId);
    }
    public async Task<IActionResult> MyLeaveStatus(string employeeId)
    {
        try
        {
            var leaveHistory = await _leaveManager.GetLeaveHistory(employeeId);
            return View(leaveHistory);
        }
        catch (Exception ex)
        {
            // Log the error
            return RedirectToAction("Error");
        }
    }
}