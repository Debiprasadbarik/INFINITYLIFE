using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using InfinityLife.DataAccess;
using InfinityLife.DataAccess.Interfaces;
using InfinityLife.DataAccess.Repositories;
using InfinityLife.Services;
//using Microsoft.Azure.Management.Storage.Models;
//using Serilog;

var builder = WebApplication.CreateBuilder(args);

//// Configure Serilog
//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .WriteTo.File("logs/infinitylife-.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
// Add services to the container
builder.Services.AddControllersWithViews();
// Register EmployeeRepository with dependency injection
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EmployeeRepository>();
// Register Services
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeRoleRepository, EmployeeRoleRepository>();
builder.Services.AddScoped<IPaySlipRepository, PaySlipRepository>();
// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
    });
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//        name: "employeePayslip",
//        pattern: "PaySlip/MyPaySlip",
//        defaults: new { controller = "PaySlip", action = "MyPaySlip" });

//    endpoints.MapControllerRoute(
//        name: "payslipByPeriod",
//        pattern: "PaySlip/View/{employeeId}/{payPeriod?}",
//        defaults: new { controller = "PaySlip", action = "View" });

//    endpoints.MapControllerRoute(
//        name: "default",
//        pattern: "{controller=Home}/{action=Index}/{id?}");
//});
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
