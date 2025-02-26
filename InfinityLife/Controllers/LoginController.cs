using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using InfinityLife.Models;
using InfinityLife.DataAccess.Repositories;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace InfinityLife.Controllers
{
    public class LoginController : Controller
    {
        private readonly EmployeeRepository _employeeRepository;
        private readonly ILogger<LoginController> _logger;
        public LoginController(EmployeeRepository employeeRepository, ILogger<LoginController> logger)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required");
                return View();
            }

            try
            {
                var user = await _employeeRepository.GetEmployeeByEmail(email);
                //bool isPasswordValid = VerifyPassword(password, user.Password);
                //_logger.LogInformation($"Password verification result for {email}: {isPasswordValid}");
                if (user != null && user.Password == password)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.EmpEmail),
                        new Claim(ClaimTypes.Role, user.EmployeeRole?.EmpRole ?? "Unknown"),
                        new Claim("EmployeeId", user.EmpId.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddHours(24)
                        });
                    _logger.LogInformation($"User {email} logged in successfully");
                    return RedirectToAction("Index", "Dashboard");
                }
                //_logger.LogWarning($"Login failed: No user found with email {email}");
                ModelState.AddModelError("", "Invalid login attempt");
                return View();
            }
            catch (Exception ex)
            {
                //#if DEBUG
                //                ModelState.AddModelError("", $"Debug - Error: {ex.Message}");
                //#else
                // ModelState.AddModelError("", "An error occurred during login. Please try again.");
                //#endif
                _logger.LogError($"Login error for {email}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(storedPassword))
                {
                    _logger.LogWarning("Stored password is null or empty");
                    return false;
                }

                return inputPassword.Equals(storedPassword);
                //var parts = storedPassword.Split(':');
                //if (parts.Length != 2)
                //{
                //    _logger.LogWarning("Stored password is not in correct format (salt:hash)");
                //    return false;
                //}
                //try {
                //    var salt = Convert.FromBase64String(parts[0]);
                //    var hashedPassword = Convert.FromBase64String(parts[1]);

                //    var newHash = KeyDerivation.Pbkdf2(
                //        password: inputPassword,
                //        salt: salt,
                //        prf: KeyDerivationPrf.HMACSHA256,
                //        iterationCount: 10000,
                //        numBytesRequested: 256 / 8);

                //    return hashedPassword.SequenceEqual(newHash);
                //}
                //catch (FormatException ex)
                //{
                //    _logger.LogError($"Password format error: {ex.Message}");
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password verification error: {ex.Message}");
                return false;
            }
        }
    }
}