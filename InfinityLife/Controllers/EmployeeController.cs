using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InfinityLife.DataAccess.Interfaces;
using InfinityLife.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace InfinityLife.Controllers
{
    [Authorize(Roles = "Director,HR")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeRoleRepository _roleRepository;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            IEmployeeRepository employeeRepository,
            IEmployeeRoleRepository roleRepository,
            ILogger<EmployeeController> logger)
        {
            _employeeRepository = employeeRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var employees = await _employeeRepository.GetAllEmployees();
                return View(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees list");
                TempData["Error"] = "Failed to retrieve employees list.";
                return View(new List<Employee>());
            }
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            try
            {
                var employee = await _employeeRepository.SearchEmployee(searchTerm);
                if (employee == null)
                {
                    return RedirectToAction("Create");
                }
                return View("Details", employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for employee with term: {SearchTerm}", searchTerm);
                TempData["Error"] = "Failed to search for employee.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Roles = await _roleRepository.GetAllRoles();
                return View(new Employee());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create employee form");
                TempData["Error"] = "Failed to load employee creation form.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            try
            {
                // Get available roles
                ModelState.Remove("EmployeeRole");
                //var availableRoles = await _roleRepository.GetAllRoles();
                //ViewBag.Roles = availableRoles;

                //// Validate if the selected role exists
                //if (!availableRoles.Any(r => r.EmpRoleId == employee.EmpRoleId))
                //{
                //    ModelState.AddModelError("EmpRoleId", "Please select a valid role");
                //    return View(employee);
                //}
                if (!ModelState.IsValid) {
                    _logger.LogWarning("ModelState is invalid. Errors: {Errors}",
                    string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));

                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            _logger.LogWarning($"Key: {modelStateKey} Error: {error.ErrorMessage}");
                        }
                    }
                }
                var availableRoles = await _roleRepository.GetAllRoles();
                ViewBag.Roles = availableRoles;

                // Validate if the selected role exists
                if (!availableRoles.Any(r => r.EmpRoleId == employee.EmpRoleId))
                {
                    ModelState.AddModelError("EmpRoleId", "Please select a valid role");
                    return View(employee);
                }
                if (ModelState.IsValid)
                    {
                    _logger.LogInformation("Attempting to create employee with email: {Email}", employee.EmpEmail);
                    employee.EmployeeRole = null;
                    var createdEmployee = await _employeeRepository.CreateEmployee(employee);
                    if (createdEmployee != null && !string.IsNullOrEmpty(createdEmployee.EmpId))
                    {
                        _logger.LogInformation("Successfully created employee with ID: {EmpId}", createdEmployee.EmpId);
                        TempData["Success"] = "Employee created successfully.";
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        _logger.LogWarning("Employee creation returned null or invalid ID");
                        ModelState.AddModelError("", "Failed to create employee. Please try again.");
                    }
                }

                // If we got this far, something failed, redisplay form
                ViewBag.Roles = await _roleRepository.GetAllRoles();
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee{Message}",ex.Message);
                TempData["Error"] = "Failed to create employee.";
                ViewBag.Roles = await _roleRepository.GetAllRoles();
                return View(employee);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeeById(id);
                if (employee == null)
                {
                    return NotFound();
                }
                var viewModel = new DashboardViewModel
                {
                    Employee= employee
                };
                //ViewBag.Roles = await _roleRepository.GetAllRoles();
                return View("Edit",viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee for edit. ID: {Id}", id);
                TempData["Error"] = "Failed to load employee details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DashboardViewModel ds)
        {
            try
            {
                ModelState.Remove("EmployeeRole");
                ModelState.Remove("Password");
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState validation failed during employee edit. Employee ID: {Id}", ds.Employee.EmpId);
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            _logger.LogWarning($"Validation Error - Field: {modelStateKey}, Error: {error.ErrorMessage}");
                        }
                    }

                    return View("Index");
                }
                ds.Employee.EmployeeRole = null;
                var existingEmployee = await _employeeRepository.GetEmployeeById(ds.Employee.EmpId);
                if (existingEmployee != null)
                {
                    ds.Employee.Password = existingEmployee.Password; // Preserve the existing password
                    ds.Employee.EmpRoleId = existingEmployee.EmpRoleId; 
                    ds.Employee.EmployeeRole = existingEmployee.EmployeeRole;
                }
                var result= await _employeeRepository.UpdateEmployee(ds.Employee);
                if (result)
                {
                    _logger.LogInformation("Successfully updated employee with ID: {Id}", ds.Employee.EmpId);
                    TempData["Success"] = "Employee updated successfully.";
                    var viewModel = new DashboardViewModel
                    {
                        Employee = ds.Employee
                    };
                    viewModel.AllEmployees = await _employeeRepository.GetAllEmployees();
                    return View("Index",viewModel);
                }
                else
                {
                    _logger.LogWarning("Employee update returned false for ID: {Id}", ds.Employee.EmpId);
                    ModelState.AddModelError("", "Failed to update employee. Please try again.");
                    ViewBag.Roles = await _roleRepository.GetAllRoles();
                    return View(ds.Employee);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee. ID: {Id}", ds.Employee.EmpId);
                TempData["Error"] = "Failed to update employee.";
                ViewBag.Roles = await _roleRepository.GetAllRoles();
                return View(ds.Employee);
            }
        }

        [HttpPost]
        [Route("Employee/Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _employeeRepository.DeleteEmployee(id);
                if (result)
                {
                    return Ok();
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee. ID: {Id}", id);
                TempData["Error"] = "Failed to delete employee.";
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditProfilePic(string id)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeeById(id);
                if (employee == null)
                {
                    return NotFound();
                }

                ViewBag.Roles = await _roleRepository.GetAllRoles();
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee for edit. ID: {Id}", id);
                TempData["Error"] = "Failed to load employee details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditProfilePic(Employee employee, IFormFile profilePicture)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    byte[] profilePictureData = null;
                    if (profilePicture != null && profilePicture.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await profilePicture.CopyToAsync(memoryStream);
                        profilePictureData = memoryStream.ToArray();
                    }

                    await _employeeRepository.UpdateEmployeeProfilePic(employee, profilePictureData);
                    TempData["Success"] = "Employee updated successfully.";
                    return RedirectToAction("Index");
                }

                ViewBag.Roles = await _roleRepository.GetAllRoles();
                return RedirectToAction("Index","Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee. ID: {Id}", employee.EmpId);
                TempData["Error"] = "Failed to update employee.";
                ViewBag.Roles = await _roleRepository.GetAllRoles();
                return View(employee);
            }
        }
        //Reset password
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email)
        {
            try
            {
                // Generate a reset password code and send it to the employee's email
                var resetPasswordCode = GenerateResetPasswordCode();
                await SendResetPasswordEmail(email, resetPasswordCode);

                // Store the reset password code and email in a temporary storage (e.g., in-memory cache, session, etc.)
                HttpContext.Session.SetString("ResetPasswordEmail", email);
                HttpContext.Session.SetString("ResetPasswordCode", resetPasswordCode);

                return RedirectToAction("VerifyResetPassword");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reset password request for email: {Email}", email);
                TempData["Error"] = "Failed to process reset password request.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult VerifyResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyResetPassword(string code, string newPassword)
        {
            var email = HttpContext.Session.GetString("ResetPasswordEmail");
            var storedCode = HttpContext.Session.GetString("ResetPasswordCode");

            if (email == null || storedCode == null || code != storedCode)
            {
                TempData["Error"] = "Invalid reset password code.";
                return RedirectToAction("VerifyResetPassword");
            }

            try
            {
                // Update the employee's password in the database
                await _employeeRepository.UpdatePassword(email, newPassword);

                // Clear the temporary storage
                HttpContext.Session.Remove("ResetPasswordEmail");
                HttpContext.Session.Remove("ResetPasswordCode");

                TempData["Success"] = "Password reset successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for email: {Email}", email);
                TempData["Error"] = "Failed to reset password.";
                return RedirectToAction("VerifyResetPassword");
            }
        }
        private string GenerateResetPasswordCode()
        {
            // Generate a random reset password code (e.g., 6-digit code)
            return new Random().Next(100000, 999999).ToString();
        }

        private Task SendResetPasswordEmail(string email, string code)
        {
            // Send an email to the employee with the reset password code
            // You can use a third-party email service or your own email infrastructure
            return Task.CompletedTask;
        }
    }
}