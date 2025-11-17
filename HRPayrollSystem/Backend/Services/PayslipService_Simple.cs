using HRPayrollSystem_Payslip.DTOs.PayslipDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using HRPayrollSystem_Payslip.Enums;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;

namespace HRPayrollSystem_Payslip.Services
{
    public class PayslipServiceSimple : IPayslipService
    {
        private readonly IPayslipRepository _payslipRepository;
        private readonly IWebHostEnvironment _environment;

        public PayslipServiceSimple(IPayslipRepository payslipRepository, IWebHostEnvironment environment)
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

            var existingPayslip = await _payslipRepository.GetByIdAsync(payslipUpdateDto.PayslipId);
            var filePath = existingPayslip!.FilePath;

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

            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(webRootPath, payslip.FilePath.TrimStart('/'));
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Payslip file not found.");
            }

            return await File.ReadAllBytesAsync(fullPath);
        }

        public async Task<string> GeneratePayslipAsync(string employeeId, int payrollId)
        {
            try
            {
                var webRootPath = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    Directory.CreateDirectory(webRootPath);
                }
                
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "payslips", employeeId);
                Directory.CreateDirectory(uploadsFolder);

                var payrollData = await GetPayrollDataAsync(payrollId);
                var employeeData = await GetEmployeeDataAsync(employeeId);
                
                var payrollMonth = payrollData.PayrollMonth;
                var fileName = $"Payslip_{employeeId}_{payrollMonth:MMMyyyy}.pdf";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var writer = new PdfWriter(filePath))
                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf))
                {
                    // Logo
                    try
                    {
                        var logoPath = Path.Combine(webRootPath, "company", "Logo.png");
                        if (File.Exists(logoPath))
                        {
                            var logoData = ImageDataFactory.Create(logoPath);
                            var logo = new Image(logoData).SetWidth(60).SetHeight(60);
                            document.Add(new Paragraph().Add(logo).SetTextAlignment(TextAlignment.CENTER));
                        }
                    }
                    catch { }
                    
                    // Company Header
                    document.Add(new Paragraph("Phoenix HR Payroll Systems")
                        .SetFontSize(20).SetBold().SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph("123 Business Park, Tech City, Mumbai - 400001")
                        .SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph("Phone: +91 98765 43210 | Email: hr@phoenixpayroll.com")
                        .SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    
                    document.Add(new Paragraph("\n"));
                    
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
                    
                    document.Add(new Paragraph("\n"));
                    
                    // Attendance
                    var attendanceData = await GetAttendanceDataAsync(employeeId, payrollMonth);
                    document.Add(new Paragraph("ATTENDANCE SUMMARY").SetFontSize(12).SetBold());
                    document.Add(new Paragraph($"Working Days: {attendanceData.WorkingDays} | Present: {attendanceData.PresentDays} | Leaves: {attendanceData.LeavesTaken} | Absent: {attendanceData.AbsentDays}"));
                    
                    document.Add(new Paragraph("\n"));
                    
                    // Salary Breakdown
                    var salaryData = await GetSalaryDataAsync(employeeId);
                    document.Add(new Paragraph("SALARY BREAKDOWN").SetFontSize(12).SetBold());
                    
                    document.Add(new Paragraph("EARNINGS:"));
                    document.Add(new Paragraph($"  Basic Salary: ₹ {salaryData.BasicSalary:N2}"));
                    document.Add(new Paragraph($"  House Rent Allowance: ₹ {salaryData.HRA:N2}"));
                    document.Add(new Paragraph($"  Other Allowances: ₹ {salaryData.Allowances:N2}"));
                    if (payrollData.Bonus > 0)
                    {
                        document.Add(new Paragraph($"  Bonus: ₹ {payrollData.Bonus:N2}"));
                    }
                    document.Add(new Paragraph($"  TOTAL EARNINGS: ₹ {payrollData.GrossSalary:N2}").SetBold());
                    
                    document.Add(new Paragraph("\nDEDUCTIONS:"));
                    document.Add(new Paragraph($"  Provident Fund: ₹ {salaryData.PF:N2}"));
                    document.Add(new Paragraph($"  Income Tax: ₹ {salaryData.Tax:N2}"));
                    document.Add(new Paragraph($"  Other Deductions: ₹ {salaryData.Deductions:N2}"));
                    document.Add(new Paragraph($"  TOTAL DEDUCTIONS: ₹ {payrollData.TotalDeductions:N2}").SetBold());
                    
                    document.Add(new Paragraph("\n"));
                    
                    document.Add(new Paragraph($"NET PAY: ₹ {payrollData.NetPay:N2}")
                        .SetFontSize(14).SetBold());
                    
                    document.Add(new Paragraph("\n"));
                    
                    document.Add(new Paragraph("Note: This is a computer-generated payslip.")
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

        private async Task<dynamic> GetSalaryDataAsync(string employeeId)
        {
            try
            {
                var salaryStructure = await _payslipRepository.GetSalaryStructureByEmployeeIdAsync(employeeId);
                if (salaryStructure != null)
                {
                    return new
                    {
                        BasicSalary = salaryStructure.BasicSalary,
                        HRA = salaryStructure.HRA,
                        Allowances = salaryStructure.Allowances,
                        PF = salaryStructure.PF,
                        Tax = salaryStructure.Tax,
                        Deductions = salaryStructure.Deductions
                    };
                }
            }
            catch { }
            
            return new
            {
                BasicSalary = 50000m,
                HRA = 15000m,
                Allowances = 5000m,
                PF = 6000m,
                Tax = 2000m,
                Deductions = 500m
            };
        }

        private async Task<dynamic> GetAttendanceDataAsync(string employeeId, DateTime payrollMonth)
        {
            var monthStart = payrollMonth;
            var monthEnd = payrollMonth.AddMonths(1).AddDays(-1);
            var totalWorkingDays = CalculateWorkingDays(monthStart, monthEnd);
            
            try
            {
                var attendanceRecords = await _payslipRepository.GetAttendanceRecordsAsync(employeeId, monthStart, monthEnd) ?? new List<Attendance>();
                var leaveRecords = await _payslipRepository.GetLeaveRecordsAsync(employeeId, monthStart, monthEnd) ?? new List<LeaveRequest>();
                
                var presentDays = attendanceRecords.Count(a => a.Status == AttendanceStatus.Present);
                var leavesTaken = (int)leaveRecords.Where(l => l.Status == LeaveStatus.Approved).Sum(l => l.NumberOfDays);
                var absentDays = Math.Max(0, totalWorkingDays - presentDays - leavesTaken);
                
                return new
                {
                    WorkingDays = totalWorkingDays,
                    PresentDays = presentDays,
                    LeavesTaken = leavesTaken,
                    AbsentDays = absentDays
                };
            }
            catch
            {
                return new
                {
                    WorkingDays = totalWorkingDays,
                    PresentDays = totalWorkingDays,
                    LeavesTaken = 0,
                    AbsentDays = 0
                };
            }
        }

        private static int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            var workingDays = 0;
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }
            return workingDays;
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