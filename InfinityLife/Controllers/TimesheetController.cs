using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InfinityLife.Models;
using InfinityLife.DataAccess;

namespace InfinityLife.Controllers
{
    [Authorize]
    public class TimeSheetController : Controller
    {
        private readonly TimeSheetRepository _repository;
        private readonly ILogger<TimeSheetController> _logger;

        public TimeSheetController(IConfiguration configuration, ILogger<TimeSheetController> logger)
        {
            _repository = new TimeSheetRepository(configuration.GetConnectionString("DefaultConnection"));
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new TimeSheetViewModel
            {
                TimeSheet = new TimeSheet(),
                Entries = new List<TimeSheetEntry>(),
                WeekendDates = GetUpcomingWeekends()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TimeSheetViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                if (viewModel.TotalHoursLogged < 45)
                {
                    ModelState.AddModelError("", "Total hours must be at least 45");
                    return View(viewModel);
                }

                // Create TimeSheet
                var timeSheetId = await _repository.CreateTimeSheet(viewModel.TimeSheet);

                // Create TimeSheet Entries
                foreach (var entry in viewModel.Entries)
                {
                    entry.TimeSheetId = timeSheetId;
                    await _repository.CreateTimeSheetEntry(entry);
                }

                TempData["Success"] = "Timesheet submitted successfully";
                return RedirectToAction("Index", "EmployeeDashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timesheet");
                ModelState.AddModelError("", "An error occurred while saving the timesheet");
                return View(viewModel);
            }
        }

        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today;

                var timeSheets = await _repository.GetTimeSheetsByDateRange(start, end);
                return View(timeSheets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheet report");
                TempData["Error"] = "Failed to load timesheet report";
                return RedirectToAction("Index", "DirectorDashboard");
            }
        }

        private List<DateTime> GetUpcomingWeekends()
        {
            var weekends = new List<DateTime>();
            var currentDate = DateTime.Today;

            // Get next 4 weekends
            while (weekends.Count < 8)
            {
                if (currentDate.DayOfWeek == DayOfWeek.Saturday ||
                    currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekends.Add(currentDate);
                }
                currentDate = currentDate.AddDays(1);
            }

            return weekends;
        }
    }
}