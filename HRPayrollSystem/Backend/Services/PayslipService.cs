using HRPayrollSystem_Payslip.DTOs.PayslipDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace HRPayrollSystem_Payslip.Services
{
    public class PayslipService : IPayslipService
    {
        private readonly IPayslipRepository _payslipRepository;
        private readonly IWebHostEnvironment _environment;

        public PayslipService(IPayslipRepository payslipRepository, IWebHostEnvironment environment)
        {
            _payslipRepository = payslipRepository;
            _environment = environment;
        }

        public async Task<IEnumerable<PayslipResponseDto>> GetAllPayslipsAsync()
        {
            var payslips = await _payslipRepository.GetAllAsync();
            return payslips.Select(MapToResponseDto);
        }

        public async Task<PayslipResponseDto?> GetPayslipByIdAsync(int id)
        {
            var payslip = await _payslipRepository.GetByIdAsync(id);
            return payslip == null ? null : MapToResponseDto(payslip);
        }

        public async Task<IEnumerable<PayslipResponseDto>> GetPayslipsByEmployeeIdAsync(string employeeId)
        {
            if (!await _payslipRepository.EmployeeExistsAsync(employeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var payslips = await _payslipRepository.GetByEmployeeIdAsync(employeeId);
            return payslips.Select(MapToResponseDto);
        }

        public async Task<PayslipResponseDto?> GetPayslipByPayrollIdAsync(int payrollId)
        {
            if (!await _payslipRepository.PayrollExistsAsync(payrollId))
            {
                throw new KeyNotFoundException("Payroll not found.");
            }

            var payslip = await _payslipRepository.GetByPayrollIdAsync(payrollId);
            return payslip == null ? null : MapToResponseDto(payslip);
        }



        public async Task<PayslipResponseDto> UpdatePayslipAsync(PayslipUpdateDto payslipUpdateDto)
        {
            if (!await _payslipRepository.ExistsAsync(payslipUpdateDto.PayslipId))
            {
                throw new KeyNotFoundException("Payslip not found.");
            }

            if (!await _payslipRepository.EmployeeExistsAsync(payslipUpdateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (!await _payslipRepository.PayrollExistsAsync(payslipUpdateDto.PayrollId))
            {
                throw new KeyNotFoundException("Payroll not found.");
            }

            if (await _payslipRepository.PayslipExistsForPayrollAsync(payslipUpdateDto.PayrollId, payslipUpdateDto.PayslipId))
            {
                throw new InvalidOperationException("Payslip already exists for this payroll.");
            }

            var existingPayslip = await _payslipRepository.GetByIdAsync(payslipUpdateDto.PayslipId);
            var filePath = existingPayslip!.FilePath;

            if (payslipUpdateDto.PayslipFile != null)
            {
                DeleteFile(filePath);
                filePath = await SaveFileAsync(payslipUpdateDto.PayslipFile, payslipUpdateDto.EmployeeId);
            }

            var payslip = new Payslip
            {
                PayslipId = payslipUpdateDto.PayslipId,
                EmployeeId = payslipUpdateDto.EmployeeId,
                PayrollId = payslipUpdateDto.PayrollId,
                FilePath = filePath,
                GeneratedDate = existingPayslip.GeneratedDate
            };

            var updatedPayslip = await _payslipRepository.UpdateAsync(payslip);
            var result = await _payslipRepository.GetByIdAsync(updatedPayslip.PayslipId);
            return MapToResponseDto(result!);
        }



        public async Task<byte[]> DownloadPayslipAsync(int id)
        {
            var payslip = await _payslipRepository.GetByIdAsync(id);
            if (payslip == null)
            {
                throw new KeyNotFoundException("Payslip not found.");
            }

            var fullPath = Path.Combine(_environment.WebRootPath, payslip.FilePath.TrimStart('/'));
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Payslip file not found.");
            }

            return await File.ReadAllBytesAsync(fullPath);
        }

        private async Task<string> SaveFileAsync(IFormFile file, string employeeId)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "payslips", employeeId);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/payslips/{employeeId}/{fileName}";
        }

        private void DeleteFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // Log error but don't throw
            }
        }

        public async Task<string> GeneratePayslipAsync(string employeeId, int payrollId)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "payslips", employeeId);
                Directory.CreateDirectory(uploadsFolder);

                // Get actual data
                var payrollData = await GetPayrollDataAsync(payrollId);
                var employeeData = await GetEmployeeDataAsync(employeeId);
                
                var payrollMonth = payrollData.PayrollMonth;
                var fileName = $"Payslip_{employeeId}_{payrollMonth:MMMyyyy}.pdf";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var writer = new PdfWriter(filePath))
                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf))
                {
                    var logoPath = Path.Combine(_environment.WebRootPath, "company", "logo.png");
                    if (File.Exists(logoPath))
                    {
                        var logoData = ImageDataFactory.Create(logoPath);
                        var logo = new Image(logoData)
                            .SetWidth(80) // adjust as needed
                            .SetFixedPosition(36, pdf.GetDefaultPageSize().GetTop() - 80);
                        // 36 = left margin, 80 = distance from top (tweak to your liking)
                        document.Add(logo);
                    }
                    // Company Header
                    document.Add(new Paragraph("Phoenix HR Payroll Systems")
                        .SetFontSize(20).SetBold().SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph("123 Business Park, Tech City, Mumbai - 400001")
                        .SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph("Phone: +91 98765 43210 | Email: hr@phoenixpayroll.com")
                        .SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph("GST: 27ABCDE1234F1Z5")
                        .SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    
                    document.Add(new Paragraph("\n"));
                    
                    // Payslip Title
                    document.Add(new Paragraph($"SALARY SLIP - {payrollMonth:MMMM yyyy}")
                        .SetFontSize(16).SetBold().SetTextAlignment(TextAlignment.CENTER));
                    
                    document.Add(new Paragraph("\n"));
                    
                    // Employee Details
                    document.Add(new Paragraph("EMPLOYEE DETAILS").SetFontSize(12).SetBold());
                    document.Add(new Paragraph($"Employee ID: {employeeData.EmployeeId}"));
                    document.Add(new Paragraph($"Employee Name: {employeeData.FirstName} {employeeData.LastName}"));
                    document.Add(new Paragraph($"Department: {employeeData.DepartmentName}"));
                    document.Add(new Paragraph($"Designation: {employeeData.RoleName}"));
                    document.Add(new Paragraph($"Date of Joining: {employeeData.DateOfJoining:dd-MMM-yyyy}"));
                    document.Add(new Paragraph($"Employment Type: {employeeData.EmploymentType}"));
                    
                    document.Add(new Paragraph("\n"));
                    
                    // Pay Period
                    document.Add(new Paragraph($"Pay Period: {payrollMonth:dd-MMM-yyyy} to {payrollMonth.AddMonths(1).AddDays(-1):dd-MMM-yyyy}"));
                    document.Add(new Paragraph($"Payment Date: {DateTime.Now:dd-MMM-yyyy}"));
                    
                    document.Add(new Paragraph("\n"));
                    
                    // Attendance Summary
                    document.Add(new Paragraph("ATTENDANCE SUMMARY").SetFontSize(12).SetBold());
                    document.Add(new Paragraph("Total Days: 31 | Working Days: 22 | Present Days: 20 | Leaves Taken: 2"));
                    
                    document.Add(new Paragraph("\n"));
                    
                    // Salary Breakdown
                    document.Add(new Paragraph("SALARY BREAKDOWN").SetFontSize(12).SetBold());
                    
                    var basicSalary = payrollData.GrossSalary * 0.6m;
                    var hra = payrollData.GrossSalary * 0.25m;
                    var allowances = payrollData.GrossSalary * 0.15m;
                    var pf = basicSalary * 0.12m;
                    var tax = payrollData.TotalDeductions - pf - 500;
                    var otherDeductions = 500m;
                    
                    document.Add(new Paragraph("EARNINGS:"));
                    document.Add(new Paragraph($"  Basic Salary: ₹ {basicSalary:N2}"));
                    document.Add(new Paragraph($"  House Rent Allowance: ₹ {hra:N2}"));
                    document.Add(new Paragraph($"  Other Allowances: ₹ {allowances:N2}"));
                    if (payrollData.Bonus > 0)
                    {
                        document.Add(new Paragraph($"  Bonus: ₹ {payrollData.Bonus:N2}"));
                    }
                    document.Add(new Paragraph($"  TOTAL EARNINGS: ₹ {payrollData.GrossSalary + payrollData.Bonus:N2}").SetBold());
                    
                    document.Add(new Paragraph("\nDEDUCTIONS:"));
                    document.Add(new Paragraph($"  Provident Fund: ₹ {pf:N2}"));
                    document.Add(new Paragraph($"  Income Tax: ₹ {tax:N2}"));
                    document.Add(new Paragraph($"  Other Deductions: ₹ {otherDeductions:N2}"));
                    document.Add(new Paragraph($"  TOTAL DEDUCTIONS: ₹ {payrollData.TotalDeductions:N2}").SetBold());
                    
                    document.Add(new Paragraph("\n"));
                    
                    // Net Pay
                    document.Add(new Paragraph($"NET PAY: ₹ {payrollData.NetPay:N2}")
                        .SetFontSize(14).SetBold());
                    
                    document.Add(new Paragraph($"Amount in Words: {ConvertToWords(payrollData.NetPay)} Rupees Only")
                        .SetFontSize(10));
                    
                    document.Add(new Paragraph("\n\n"));
                    
                    // Footer
                    document.Add(new Paragraph("Note: This is a computer-generated payslip and does not require a signature.")
                        .SetFontSize(8).SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph($"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm}")
                        .SetFontSize(8).SetTextAlignment(TextAlignment.CENTER));
                }

                return $"/uploads/payslips/{employeeId}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"PDF generation failed: {ex.Message}", ex);
            }
        }



        private Cell CreateCell(string content, PdfFont font)
        {
            return new Cell().Add(new Paragraph(content).SetFont(font).SetFontSize(9))
                           .SetPadding(5).SetBorder(new SolidBorder(1));
        }

        private Cell CreateHeaderCell(string content, PdfFont font)
        {
            return new Cell().Add(new Paragraph(content).SetFont(font).SetFontSize(9))
                           .SetPadding(5).SetBorder(new SolidBorder(1))
                           .SetBackgroundColor(ColorConstants.LIGHT_GRAY);
        }

        private string ConvertToWords(decimal amount)
        {
            // Simple implementation for demo
            var intAmount = (int)amount;
            if (intAmount < 1000) return "Less than One Thousand";
            if (intAmount < 100000) return $"{intAmount / 1000} Thousand {intAmount % 1000}";
            return $"{intAmount / 100000} Lakh {(intAmount % 100000) / 1000} Thousand";
        }

        private async Task<dynamic> GetPayrollDataAsync(int payrollId)
        {
            var payroll = await _payslipRepository.GetPayrollByIdAsync(payrollId);
            if (payroll != null)
            {
                return new
                {
                    PayrollId = payroll.PayrollId,
                    PayrollMonth = payroll.PayrollMonth,
                    GrossSalary = payroll.GrossSalary,
                    TotalDeductions = payroll.TotalDeductions,
                    Bonus = payroll.Bonus,
                    NetPay = payroll.NetPay
                };
            }
            
            return new
            {
                PayrollId = payrollId,
                PayrollMonth = DateTime.Now.AddMonths(-1),
                GrossSalary = 75000m,
                TotalDeductions = 15000m,
                Bonus = 0m,
                NetPay = 60000m
            };
        }

        private async Task<dynamic> GetEmployeeDataAsync(string employeeId)
        {
            // Get actual employee data from repository
            var employee = await _payslipRepository.GetEmployeeByIdAsync(employeeId);
            if (employee != null)
            {
                return new
                {
                    EmployeeId = employee.EmployeeId,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    DepartmentName = employee.Department?.DepartmentName ?? "N/A",
                    RoleName = employee.Role?.RoleName ?? "N/A",
                    DateOfJoining = employee.DateOfJoining,
                    EmploymentType = employee.EmploymentType.ToString()
                };
            }
            
            // Fallback data if employee not found
            return new
            {
                EmployeeId = employeeId,
                FirstName = "Employee",
                LastName = "Name",
                DepartmentName = "Department",
                RoleName = "Role",
                DateOfJoining = DateTime.Now,
                EmploymentType = "FullTime"
            };
        }

        private static PayslipResponseDto MapToResponseDto(Payslip payslip)
        {
            return new PayslipResponseDto
            {
                PayslipId = payslip.PayslipId,
                EmployeeId = payslip.EmployeeId,
                EmployeeName = $"{payslip.Employee?.FirstName} {payslip.Employee?.LastName}".Trim(),
                PayrollId = payslip.PayrollId,
                PayrollMonth = payslip.Payroll?.PayrollMonth ?? DateTime.MinValue,
                FilePath = payslip.FilePath,
                FileName = Path.GetFileName(payslip.FilePath),
                GeneratedDate = payslip.GeneratedDate,
                NetPay = payslip.Payroll?.NetPay ?? 0
            };
        }
    }
}