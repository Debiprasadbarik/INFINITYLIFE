using InfinityLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace InfinityLife.Controllers
{
    public class DirectorDashboardController : Controller
    {
        private readonly LeaveManager _leaveManager;
        private readonly string? _connectionString;

        public DirectorDashboardController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection"); ;
            _leaveManager = new LeaveManager(_connectionString);
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                PendingLeaveRequests = await _leaveManager.GetPendingLeaveRequests(),
                LeaveBalances = await _leaveManager.GetAllLeaveBalances(),
            };

            return View(viewModel);
        }

        public IActionResult PendingRequests()
        {
            return RedirectToAction("Index", "Leave");
        }
    }
}
