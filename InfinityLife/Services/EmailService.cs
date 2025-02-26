using InfinityLife.Models;
using System.Net;
using System.Net.Mail;

namespace InfinityLife.Services
{
    public class EmailService
    {
        private readonly string? _smtpHost;
        private readonly int _smtpPort;
        private readonly string? _smtpUsername;
        private readonly string? _smtpPassword;
        private readonly string? _directorEmail;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration)
        {
            _smtpHost = configuration["EmailSettings:SmtpHost"];
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
            _smtpUsername = configuration["EmailSettings:Username"];
            _smtpPassword = configuration["EmailSettings:Password"];
            _directorEmail = configuration["EmailSettings:DirectorEmail"];
            _enableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"]);
        }

        public async Task SendLeaveRequestEmail(Leave leave, string employeeName, string employeeEmail)
        {
            var subject = $"New Leave Request from {employeeName}";
            var body = $@"
                <h2>New Leave Request Details:</h2>
                <p>Employee: {employeeName}</p>
                <p>Employee ID: {leave.EmployeeId}</p>
                <p>From Date: {leave.FromDate:dd-MMM-yyyy}</p>
                <p>To Date: {leave.ToDate:dd-MMM-yyyy}</p>
                <p>Reason: {leave.Reason}</p>
                <p>Request Date: {DateTime.Now}</p>";

            await SendEmailAsync(_directorEmail, subject, body);
        }

        public async Task SendLeaveStatusUpdateEmail(Leave leave, string employeeName, string employeeEmail)
        {
            var subject = $"Leave Request {leave.Status}";
            var body = $@"
                <h2>Your Leave Request Status Has Been Updated</h2>
                <p>Status: {leave.Status}</p>
                <p>From Date: {leave.FromDate:dd-MMM-yyyy}</p>
                <p>To Date: {leave.ToDate:dd-MMM-yyyy}</p>
                <p>Comments: {leave.ResponseComment}</p>";

            await SendEmailAsync(employeeEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.EnableSsl = _enableSsl;

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_smtpUsername);
                        message.To.Add(toEmail);
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = true;

                        await client.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw it to prevent disrupting the main flow
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}