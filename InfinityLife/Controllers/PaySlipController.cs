using InfinityLife.DataAccess.Interfaces;
using InfinityLife.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace InfinityLife.Controllers
{


    [Authorize]
    public class PaySlipController : Controller
    {
        private readonly IPaySlipRepository _paySlipRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<PaySlipController> _logger;

        public PaySlipController(
            IPaySlipRepository paySlipRepository,
            IEmployeeRepository employeeRepository,
            ILogger<PaySlipController> logger)
        {
            _paySlipRepository = paySlipRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }
        [Authorize(Roles = "Director,Accountant")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var employees = await _employeeRepository.GetAllEmployees();
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var viewModel = new PaySlipIndexViewModel
                {
                    Employees = employees,
                    CurrentMonth = currentMonth,
                    CurrentYear = currentYear
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading PaySlip Index page");
                TempData["Error"] = "Failed to load employee list.";
                return View(new PaySlipIndexViewModel());
            }
        }
        [HttpGet]
        [Authorize(Roles = "Director,Employee,HR,Accountant")]
        public async Task<IActionResult> View(string? employeeId, DateTime payPeriod)
        {
            try
            {
                // If no employee ID is provided, get it from the current user's claims
                if (string.IsNullOrEmpty(employeeId) && User.IsInRole("Employee"))
                {
                    employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(employeeId))
                    {
                        _logger.LogWarning("No EmployeeId claim found for user");
                        return RedirectToAction("Login", "Login");
                    }
                }
                if (string.IsNullOrEmpty(employeeId))
                {
                    _logger.LogWarning("View attempt with null or empty employeeId");
                    return BadRequest("Employee ID is required");
                }
                //if (User.IsInRole("Employee"))
                //{
                //    var currentUserEmployeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                //    _logger.LogWarning(currentUserEmployeeId);
                //    if (currentUserEmployeeId != employeeId)
                //    {
                //        return Forbid();
                //    }
                //}
                if (payPeriod == default(DateTime))
                {
                    payPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                var employee = await _employeeRepository.GetEmployeeById(employeeId);
                if (employee == null)
                {
                    _logger.LogWarning("Employee not found with ID: {EmployeeId}", employeeId);
                    return NotFound();
                }
                var paySlip = await _paySlipRepository.GetPaySlipById(employeeId, payPeriod);
                if (paySlip == null)
                {
                    _logger.LogWarning("PaySlip not found for employee {EmployeeId}", employeeId);
                    return NotFound();
                }

                // Create PaySlip view model directly from employee data
                var viewModel = new PaySlipViewModel
                {
                    EmployeeId = employee.EmpId,
                    EmployeeName = $"{paySlip.Employee.EmpFirstName} {paySlip.Employee.EmpLastName}",
                    EmployeeMail = paySlip.Employee.EmpEmail,
                    PayPeriod = payPeriod,
                    GeneratedDate = DateTime.Now,
                    BasicSalary = paySlip.BasicSalary,
                    HRA = paySlip.HRA,
                    Conveyance = paySlip.Conveyance,
                    OtherAllowances = paySlip.OtherAllowances,
                    PF = paySlip.PF,
                    ESIC = paySlip.ESIC,
                    ProfessionalTax = paySlip.ProfessionalTax,
                    IncomeTax = paySlip.IncomeTax,
                    GrossSalary = paySlip.BasicSalary + paySlip.HRA + paySlip.Conveyance + paySlip.OtherAllowances,
                };
                viewModel.TotalDeductions = paySlip.PF + paySlip.ESIC + paySlip.ProfessionalTax + paySlip.IncomeTax;
                viewModel.NetSalary = viewModel.GrossSalary - viewModel.TotalDeductions;

                return View("View",viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pay slip for employee {EmployeeId}", employeeId);
                return StatusCode(500, "Error generating pay slip");
            }
        }
        //[HttpGet]
        //[Authorize(Roles = "Employee")]
        //public async Task<IActionResult> MyPaySlip()
        //{
        //    var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    return await View(employeeId, null);
        //}
        [HttpGet]
        [Authorize(Roles = "Director,Accountant")]
        public async Task<IActionResult> Generate(string employeeId)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeeById(employeeId);
                if (employee == null)
                {
                    _logger.LogWarning("Employee not found with ID: {EmployeeId}", employeeId);
                    TempData["Error"] = "Employee not found.";
                    return RedirectToAction("Index");
                }
                var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var existingPaySlips = await _paySlipRepository.GetPaySlipsByEmployeeId(employeeId);
                if (existingPaySlips.Any(p => p.PayPeriod.Month == currentPeriod.Month && p.PayPeriod.Year == currentPeriod.Year))
                {
                    _logger.LogWarning("Payslip already exists for employee {EmployeeId} for period {Period}", employeeId, currentPeriod);
                    TempData["Error"] = "Payslip already exists for this month.";
                    return RedirectToAction("Index");
                }
                var paySlip = new PaySlip
                {
                    EmployeeId = employee.EmpId,
                    BasicSalary = employee.BasicSalary,
                    HRA = employee.HRA,
                    Conveyance = employee.Conveyance,
                    OtherAllowances = employee.OtherAllowances,
                    PF = employee.PF,
                    ESIC = employee.ESIC,
                    ProfessionalTax = employee.ProfessionalTax,
                    IncomeTax = employee.IncomeTax,
                    GeneratedDate = DateTime.Now,
                    PayPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
                    // GeneratedBy = User.FindFirstValue(ClaimTypes.Name)
                };
                var generatedPaySlip = await _paySlipRepository.CreatePaySlip(paySlip);
                if (generatedPaySlip != null)
                {
                    _logger.LogInformation("Successfully generated payslip for employee {EmployeeId}", employeeId);
                    TempData["Success"] = "Pay slip generated successfully.";
                    return RedirectToAction("View", new { id = generatedPaySlip.PaySlipId });
                }
                _logger.LogError("Failed to generate payslip for employee {EmployeeId}", employeeId);
                TempData["Error"] = "Failed to generate pay slip.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payslip for employee {EmployeeId}", employeeId);
                TempData["Error"] = "An error occurred while generating the pay slip.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director,Accountant")]
        public async Task<IActionResult> Generate(PaySlipViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var employee = await _employeeRepository.GetEmployeeById(model.EmployeeId);
                    if (employee == null)
                    {
                        ModelState.AddModelError("EmployeeId", "Selected employee not found");
                        var Employee = await _employeeRepository.GetAllEmployees();
                        ViewBag.EmpId = new SelectList(Employee.Select(e => new
                        {
                            empId = e.EmpId,
                            FullName = $"{e.EmpFirstName} {e.EmpLastName}"
                        }), "empId", "FullName");
                        return View(model);
                    }
                    var existingPaySlips = await _paySlipRepository.GetPaySlipsByEmployeeId(model.EmployeeId);
                    if (existingPaySlips.Any(p => p.PayPeriod == model.PayPeriod))
                    {
                        ModelState.AddModelError("", "A pay slip already exists for this employee in the selected month and year");
                        await LoadEmployeeSelectList(model.EmployeeId);
                        return View(model);
                    }
                    var paySlip = new PaySlip
                    {
                        EmployeeId = model.EmployeeId,
                        PayPeriod = model.PayPeriod,
                        BasicSalary = model.BasicSalary,
                        HRA = model.HRA,
                        Conveyance = model.Conveyance,
                        OtherAllowances = model.OtherAllowances,
                        Bonus = model.Bonus,
                        PF = model.PF,
                        ESIC = model.ESIC,
                        ProfessionalTax = model.ProfessionalTax,
                        IncomeTax = model.IncomeTax,
                        GeneratedDate = model.GeneratedDate,
                        //UpdatedBy = User.FindFirstValue(ClaimTypes.Name),
                    };

                    var generatedPaySlip = await _paySlipRepository.CreatePaySlip(paySlip);
                    if (generatedPaySlip != null)
                    {
                        TempData["Success"] = "Pay slip generated successfully.";
                        return RedirectToAction("View", new { id = generatedPaySlip.PaySlipId });
                    }
                }

                var employees = await _employeeRepository.GetAllEmployees();
                ViewBag.Employee = new SelectList(employees.Select(e => new
                {
                    Value = e.EmpId,
                    Text = $"{e.EmpId} - {e.EmpFirstName} {e.EmpLastName}"
                }), "Value", "Text", model.EmployeeId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pay slip");
                TempData["Error"] = "Failed to generate pay slip.";
                var employees = await _employeeRepository.GetAllEmployees();
                ViewBag.Employee = new SelectList(employees.Select(e => new
                {
                    Value = e.EmpId,
                    Text = $"{e.EmpId} - {e.EmpFirstName} {e.EmpLastName}"
                }), "Value", "Text", model.EmployeeId);
                return View(model);
            }
        }
        private async Task LoadEmployeeSelectList(string selectedEmployeeId = null)
        {
            var employees = await _employeeRepository.GetAllEmployees();
            ViewBag.Employee = new SelectList(employees.Select(e => new
            {
                Value = e.EmpId,
                Text = $"{e.EmpId} - {e.EmpFirstName} {e.EmpLastName}"
            }), "Value", "Text", selectedEmployeeId);
        }

        [HttpGet]
        [Authorize(Roles = "Director,Employee,HR,Accountant")]
        public async Task<IActionResult> Download(string employeeId, DateTime payPeriod)
        {
            MemoryStream pdfStream = null;
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    employeeId = User.FindFirstValue("EmployeeId");
                    if (string.IsNullOrEmpty(employeeId))
                    {
                        return BadRequest("Employee ID is required");
                    }
                }
                // Check if the current user has access to this payslip
                if (User.IsInRole("Employee"))
                {
                    var currentUserEmployeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (currentUserEmployeeId != employeeId)
                    {
                        _logger.LogWarning("Unauthorized download attempt for employee {EmployeeId} by user {CurrentUserId}",
                    employeeId, currentUserEmployeeId);
                        //return Forbid();
                    }
                }
                var employee = await _employeeRepository.GetEmployeeById(employeeId);
                if (employee == null)
                {
                    return NotFound();
                }
                var paySlip = await _paySlipRepository.GetPaySlipById(employeeId, payPeriod);
                if (paySlip == null)
                {
                    _logger.LogWarning("PaySlip not found for employee {EmployeeId} and period {PayPeriod}",
                employeeId, payPeriod);
                    return NotFound();
                }

                //    // Generate PDF and return file
                //    var pdfStream = await GeneratePaySlipPdf(paySlip);

                //    _logger.LogInformation("Successfully generated PDF for employee {EmployeeId} for period {PayPeriod}",
                //        employeeId, payPeriod);
                //    return File(pdfStream.ToArray(), "application/pdf", fileName,true);
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogError(ex, "Error downloading pay slip for employee {EmployeeId}", employeeId);
                //    return StatusCode(500, "An error occurred while generating the pay slip PDF. Please try again later.");

                var viewModel = new PaySlipViewModel
                {
                    EmployeeId = employee.EmpId,
                    EmployeeName = $"{paySlip.Employee.EmpFirstName} {paySlip.Employee.EmpLastName}",
                    EmployeeMail = paySlip.Employee.EmpEmail,
                    PayPeriod = payPeriod,
                    GeneratedDate = DateTime.Now,
                    BasicSalary = paySlip.BasicSalary,
                    HRA = paySlip.HRA,
                    Conveyance = paySlip.Conveyance,
                    OtherAllowances = paySlip.OtherAllowances,
                    PF = paySlip.PF,
                    ESIC = paySlip.ESIC,
                    ProfessionalTax = paySlip.ProfessionalTax,
                    IncomeTax = paySlip.IncomeTax,
                    GrossSalary = paySlip.BasicSalary + paySlip.HRA + paySlip.Conveyance + paySlip.OtherAllowances,
                };
                viewModel.TotalDeductions = paySlip.PF + paySlip.ESIC + paySlip.ProfessionalTax + paySlip.IncomeTax;
                viewModel.NetSalary = viewModel.GrossSalary - viewModel.TotalDeductions;
                pdfStream = await GeneratePaySlipPdf(viewModel);
                var pdfBytes = pdfStream.ToArray();
                var fileName = $"PaySlip_{paySlip.PayPeriod:yyyyMM}_{paySlip.EmployeeId}.pdf";
                //using var pdfStream = await GeneratePaySlipPdf(viewModel);
                //return File(pdfStream.ToArray(), "application/pdf", $"PaySlip_{payPeriod:yyyyMM}_{employeeId}.pdf");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error downloading pay slip for employee {EmployeeId}: {Message}", employeeId, ex.Message);
                //TempData["Error"] = "Failed to download pay slip. Please try again.";
                //return RedirectToAction("View", new { employeeId, payPeriod });
                _logger.LogError(ex, "Error downloading pay slip for employee {EmployeeId}: {Message}", employeeId, ex.Message);
                TempData["Error"] = "Failed to download pay slip. Please try again.";
                return RedirectToAction("View", new { employeeId, payPeriod });
            }
            finally
            {
                pdfStream?.Dispose();
            }
        }

        private async Task<MemoryStream> GeneratePaySlipPdf(PaySlipViewModel model)
        {
            var outputStream = new MemoryStream();
            var tempStream = new MemoryStream();
            Document document = null;
            PdfWriter writer = null;

            try
            {
                document = new Document(PageSize.A4, 50, 50, 25, 25);
                writer = PdfWriter.GetInstance(document, tempStream);

                document.Open();
                try
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "image.png");
                    
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
                        // Scale logo to reasonable size (e.g., width of 100 units)
                        float maxWidth = 100;
                        float scale = maxWidth / logo.Width;
                        logo.ScalePercent(scale * 200);

                        // Center the logo
                        logo.Alignment = Element.ALIGN_CENTER;
                        document.Add(logo);
                        //document.Add(new Paragraph("\n"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading company logo");
                    // Continue without the logo if there's an error
                }
                //var phrase = new Phrase();
                //// "Infinity" in #0070bc
                //var infinityFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                //infinityFont.Color = new BaseColor(0, 112, 188); // #0070bc in RGB
                //phrase.Add(new Chunk("Infinity", infinityFont));

                //// "Minds" in black
                //var mindsFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                //mindsFont.Color = BaseColor.BLACK;
                //phrase.Add(new Chunk("Minds", mindsFont));

                //// "Consulting" in black
                //phrase.Add(new Chunk(" Consulting", mindsFont));

                //var headerParagraph = new Paragraph(phrase);
                //headerParagraph.Alignment = Element.ALIGN_CENTER;
                //document.Add(headerParagraph);
                //document.Add(new Paragraph("\n"));

                // Add payslip header
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var title = new Paragraph($"Pay Slip for {model.PayPeriod:MMMM yyyy}", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph("\n"));

                // Employee details
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                var empDetails = new PdfPTable(2);
                empDetails.WidthPercentage = 100;
                empDetails.SetWidths(new float[] { 1f, 2f });

                AddTableRow(empDetails, "Employee Name:", model.EmployeeName, normalFont);
                AddTableRow(empDetails, "Employee ID:", model.EmployeeId, normalFont);
                AddTableRow(empDetails, "Email:", model.EmployeeMail, normalFont);
                document.Add(empDetails);
                document.Add(new Paragraph("\n"));

                // Create tables for earnings and deductions
                var earningsTable = new PdfPTable(2);
                earningsTable.WidthPercentage = 100;
                earningsTable.SetWidths(new float[] { 2f, 1f });

                // Add Earnings header
                var headerCell = new PdfPCell(new Phrase("Earnings", titleFont));
                headerCell.Colspan = 2;
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell.BackgroundColor = new BaseColor(240, 240, 240);
                headerCell.Padding = 8;
                earningsTable.AddCell(headerCell);

                // Add earnings details
                AddTableRow(earningsTable, "Basic Salary", model.BasicSalary.ToString("C"), normalFont);
                AddTableRow(earningsTable, "HRA", model.HRA.ToString("C"), normalFont);
                AddTableRow(earningsTable, "Conveyance", model.Conveyance.ToString("C"), normalFont);
                AddTableRow(earningsTable, "Other Allowances", model.OtherAllowances.ToString("C"), normalFont);
                AddTableRow(earningsTable, "Gross Salary", model.GrossSalary.ToString("C"), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));

                document.Add(earningsTable);
                document.Add(new Paragraph("\n"));

                // Deductions table
                var deductionsTable = new PdfPTable(2);
                deductionsTable.WidthPercentage = 100;
                deductionsTable.SetWidths(new float[] { 2f, 1f });

                headerCell = new PdfPCell(new Phrase("Deductions", titleFont));
                headerCell.Colspan = 2;
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell.BackgroundColor = new BaseColor(240, 240, 240);
                headerCell.Padding = 8;
                deductionsTable.AddCell(headerCell);

                AddTableRow(deductionsTable, "PF", model.PF.ToString("C"), normalFont);
                AddTableRow(deductionsTable, "ESIC", model.ESIC.ToString("C"), normalFont);
                AddTableRow(deductionsTable, "Professional Tax", model.ProfessionalTax.ToString("C"), normalFont);
                AddTableRow(deductionsTable, "Income Tax", model.IncomeTax.ToString("C"), normalFont);
                AddTableRow(deductionsTable, "Total Deductions", model.TotalDeductions.ToString("C"), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));

                document.Add(deductionsTable);
                document.Add(new Paragraph("\n"));

                // Net Salary
                var netSalaryTable = new PdfPTable(2);
                netSalaryTable.WidthPercentage = 100;
                netSalaryTable.SetWidths(new float[] { 2f, 1f });

                headerCell = new PdfPCell(new Phrase("Net Salary", titleFont));
                headerCell.Colspan = 2;
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell.BackgroundColor = new BaseColor(217, 247, 255);
                headerCell.Padding = 8;
                netSalaryTable.AddCell(headerCell);

                AddTableRow(netSalaryTable, "Net Salary", model.NetSalary.ToString("C"), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));
                document.Add(netSalaryTable);

                // Add footer
                document.Add(new Paragraph("\n"));
                var footer = new Paragraph($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont);
                footer.Alignment = Element.ALIGN_RIGHT;
                document.Add(footer);

                document.Close();
                writer.Close();

                // Copy the generated PDF to the output stream
                var pdfBytes = tempStream.ToArray();
                await outputStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                outputStream.Position = 0;

                return outputStream;
            }
            finally
            {
                // Clean up resources
                if (document != null && document.IsOpen())
                {
                    document.Close();
                }
                if (writer != null)
                {
                    writer.Close();
                }
                tempStream.Dispose();
            }
        }
        private void AddTableRow(PdfPTable table, string label, string value, Font font)
        {
            var cell1 = new PdfPCell(new Phrase(label, font));
            cell1.Border = Rectangle.NO_BORDER;
            cell1.PaddingTop = 5;
            cell1.PaddingBottom = 5;
            table.AddCell(cell1);

            var cell2 = new PdfPCell(new Phrase(value, font));
            cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
            cell2.Border = Rectangle.NO_BORDER;
            cell2.PaddingTop = 5;
            cell2.PaddingBottom = 5;
            table.AddCell(cell2);
        }
    }
}
