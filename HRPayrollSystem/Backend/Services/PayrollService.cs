using HRPayrollSystem_Payslip.DTOs.PayrollDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IPayslipService _payslipService;
        private readonly ILogger<PayrollService> _logger;

        public PayrollService(IPayrollRepository payrollRepository, IPayslipService payslipService, ILogger<PayrollService> logger)
        {
            _payrollRepository = payrollRepository;
            _payslipService = payslipService;
            _logger = logger;
        }

        public async Task<IEnumerable<PayrollResponseDto>> GetAllPayrollsAsync()
        {
            var payrolls = await _payrollRepository.GetAllAsync();
            return payrolls.Select(MapToResponseDto);
        }

        public async Task<PayrollResponseDto?> GetPayrollByIdAsync(int id)
        {
            var payroll = await _payrollRepository.GetByIdAsync(id);
            return payroll == null ? null : MapToResponseDto(payroll);
        }

        public async Task<IEnumerable<PayrollResponseDto>> GetPayrollsByEmployeeIdAsync(string employeeId)
        {
            if (!await _payrollRepository.EmployeeExistsAsync(employeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var payrolls = await _payrollRepository.GetByEmployeeIdAsync(employeeId);
            return payrolls.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<PayrollResponseDto>> GetPayrollsByMonthAsync(DateTime month)
        {
            var payrolls = await _payrollRepository.GetByMonthAsync(month);
            return payrolls.Select(MapToResponseDto);
        }



        public async Task<PayrollResponseDto> UpdatePayrollAsync(PayrollUpdateDto payrollUpdateDto)
        {
            if (!await _payrollRepository.ExistsAsync(payrollUpdateDto.PayrollId))
            {
                throw new KeyNotFoundException("Payroll not found.");
            }

            if (!await _payrollRepository.EmployeeExistsAsync(payrollUpdateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (await _payrollRepository.PayrollExistsForMonthAsync(payrollUpdateDto.EmployeeId, payrollUpdateDto.PayrollMonth, payrollUpdateDto.PayrollId))
            {
                throw new InvalidOperationException("Payroll already exists for this employee in the specified month.");
            }

            var payroll = new Payroll
            {
                PayrollId = payrollUpdateDto.PayrollId,
                EmployeeId = payrollUpdateDto.EmployeeId,
                PayrollMonth = payrollUpdateDto.PayrollMonth,
                GrossSalary = payrollUpdateDto.GrossSalary,
                TotalDeductions = payrollUpdateDto.TotalDeductions,
                NetPay = payrollUpdateDto.NetPay,
                PaymentDate = payrollUpdateDto.PaymentDate,
                PaymentStatus = payrollUpdateDto.PaymentStatus
            };

            var updatedPayroll = await _payrollRepository.UpdateAsync(payroll);
            var result = await _payrollRepository.GetByIdAsync(updatedPayroll.PayrollId);
            return MapToResponseDto(result!);
        }

        public async Task<bool> DeletePayrollAsync(int id)
        {
            if (!await _payrollRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException("Payroll not found.");
            }

            // Payslips will be cascade deleted with payroll
            return await _payrollRepository.DeleteAsync(id);
        }

        public async Task<PayrollResponseDto> GeneratePayrollAsync(DTOs.PayrollDTO.PayrollGenerateDto payrollGenerateDto)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (!await _payrollRepository.EmployeeExistsAsync(payrollGenerateDto.EmployeeId))
                {
                    _logger.LogError("Employee {EmployeeId} not found for payroll generation", payrollGenerateDto.EmployeeId);
                    throw new KeyNotFoundException("Employee not found.");
                }

                var month = new DateTime(payrollGenerateDto.Month.Year, payrollGenerateDto.Month.Month, 1);
                
                if (await _payrollRepository.PayrollExistsForMonthAsync(payrollGenerateDto.EmployeeId, month))
                {
                    _logger.LogError("Payroll already exists for employee {EmployeeId} in month {Month}", payrollGenerateDto.EmployeeId, month.ToString("yyyy-MM"));
                    throw new InvalidOperationException("Payroll already exists for this employee in the specified month.");
                }

                var salaryStructure = await _payrollRepository.GetEmployeeSalaryStructureAsync(payrollGenerateDto.EmployeeId);
                if (salaryStructure == null)
                {
                    throw new InvalidOperationException("Employee does not have a salary structure defined.");
                }

                // Calculate salary using new logic
                var salaryCalculation = await CalculateNewSalaryAsync(payrollGenerateDto.EmployeeId, month, salaryStructure, payrollGenerateDto.BonusAmount ?? 0, payrollGenerateDto.AdditionalDeductions ?? 0);

                var payroll = new Payroll
                {
                    EmployeeId = payrollGenerateDto.EmployeeId,
                    PayrollMonth = month,
                    GrossSalary = salaryCalculation.GrossSalary,
                    TotalDeductions = salaryCalculation.TotalDeductions,
                    Bonus = salaryCalculation.Bonus,
                    NetPay = salaryCalculation.NetPay,
                    PaymentDate = DateTime.Today.AddDays(5),
                    PaymentStatus = payrollGenerateDto.PaymentStatus
                };

                var createdPayroll = await _payrollRepository.CreateAsync(payroll);
                
                // Auto-generate payslip with error handling
                try
                {
                    var payslipPath = await _payslipService.GeneratePayslipAsync(payrollGenerateDto.EmployeeId, createdPayroll.PayrollId);
                    
                    // Create payslip database record
                    var payslip = new Payslip
                    {
                        EmployeeId = payrollGenerateDto.EmployeeId,
                        PayrollId = createdPayroll.PayrollId,
                        FilePath = payslipPath,
                        GeneratedDate = DateTime.Now
                    };
                    await _payrollRepository.CreatePayslipAsync(payslip);
                    _logger.LogInformation("Payslip generated successfully for payroll {PayrollId}", createdPayroll.PayrollId);
                }
                catch (Exception payslipEx)
                {
                    _logger.LogError(payslipEx, "Failed to generate payslip for payroll {PayrollId}, but payroll creation succeeded", createdPayroll.PayrollId);
                    // Continue without failing the entire payroll creation
                }
                
                var result = await _payrollRepository.GetByIdAsync(createdPayroll.PayrollId);
                stopwatch.Stop();
                _logger.LogInformation("Payroll generated for employee {EmployeeId} with net pay {NetPay} in {ElapsedMs}ms", payrollGenerateDto.EmployeeId, salaryCalculation.NetPay, stopwatch.ElapsedMilliseconds);
                return MapToResponseDto(result!);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to generate payroll for employee {EmployeeId} after {ElapsedMs}ms", payrollGenerateDto.EmployeeId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        private async Task<SalaryCalculationDto> CalculateNewSalaryAsync(string employeeId, DateTime month, SalaryStructure salaryStructure, decimal bonus, decimal additionalDeductions)
        {
            // Get employee joining date
            var employee = await _payrollRepository.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }
            
            // Calculate working period: from joining date or month start, whichever is later
            var monthStart = month;
            var monthEnd = month.AddMonths(1).AddDays(-1);
            var workingPeriodStart = employee.DateOfJoining > monthStart ? employee.DateOfJoining : monthStart;
            var workingPeriodEnd = monthEnd;
            
            // Calculate total days in working period
            var totalDaysInPeriod = (workingPeriodEnd - workingPeriodStart).Days + 1;
            var dailyBasicSalary = salaryStructure.BasicSalary / totalDaysInPeriod;
            
            // Get attendance and leave data for the working period
            var attendanceRecords = await _payrollRepository.GetAttendanceRecordsAsync(employeeId, workingPeriodStart, workingPeriodEnd);
            var leaveRecords = await _payrollRepository.GetLeaveRecordsAsync(employeeId, workingPeriodStart, workingPeriodEnd);
            
            _logger.LogInformation("Calculating salary for {EmployeeId} - Period: {StartDate} to {EndDate}, Total Days: {TotalDays}, Daily Basic: {DailyBasic}", 
                employeeId, workingPeriodStart.ToString("yyyy-MM-dd"), workingPeriodEnd.ToString("yyyy-MM-dd"), totalDaysInPeriod, dailyBasicSalary);
            
            decimal totalBasicDeduction = 0;
            int weekdaysProcessed = 0;
            int presentDays = 0;
            int paidLeaveDays = 0;
            int absentDays = 0;
            
            // Process each weekday in the working period
            for (var date = workingPeriodStart; date <= workingPeriodEnd; date = date.AddDays(1))
            {
                // Skip weekends
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                
                weekdaysProcessed++;
                
                // Check if employee was present
                var attendance = attendanceRecords.FirstOrDefault(a => a.Date.Date == date.Date);
                if (attendance != null && attendance.Status == Enums.AttendanceStatus.Present)
                {
                    presentDays++;
                    continue; // Present - no deduction
                }
                
                // Check for paid leave (Casual, Sick, Earned, Maternity, Paternity)
                var paidLeave = leaveRecords.FirstOrDefault(l => 
                    l.Status == Enums.LeaveStatus.Approved &&
                    date >= l.FromDate.Date && date <= l.ToDate.Date &&
                    (l.LeaveType == Enums.LeaveType.Casual || 
                     l.LeaveType == Enums.LeaveType.Sick || 
                     l.LeaveType == Enums.LeaveType.Earned ||
                     l.LeaveType == Enums.LeaveType.Maternity ||
                     l.LeaveType == Enums.LeaveType.Paternity));
                
                if (paidLeave != null)
                {
                    paidLeaveDays++;
                    // For paid leave, no deduction regardless of half day or full day
                    continue;
                }
                
                // If no attendance record OR absent OR unpaid leave - deduct from basic salary
                absentDays++;
                totalBasicDeduction += dailyBasicSalary;
            }
            
            _logger.LogInformation("Salary calculation for {EmployeeId}: Weekdays: {Weekdays}, Present: {Present}, Paid Leave: {PaidLeave}, Absent: {Absent}, Basic Deduction: {Deduction}", 
                employeeId, weekdaysProcessed, presentDays, paidLeaveDays, absentDays, totalBasicDeduction);
            
            // Calculate final amounts - only basic salary is adjusted, HRA and Allowances remain full
            var adjustedBasicSalary = salaryStructure.BasicSalary - totalBasicDeduction;
            var fullHRA = salaryStructure.HRA; // Always full amount
            var fullAllowances = salaryStructure.Allowances; // Always full amount
            var grossSalary = adjustedBasicSalary + fullHRA + fullAllowances;
            var totalDeductions = salaryStructure.Deductions + salaryStructure.PF + salaryStructure.Tax + additionalDeductions;
            var netPay = grossSalary + bonus - totalDeductions;
            
            return new SalaryCalculationDto
            {
                AdjustedBasicSalary = adjustedBasicSalary,
                HRA = fullHRA,
                Allowances = fullAllowances,
                GrossSalary = grossSalary,
                Bonus = bonus,
                TotalDeductions = totalDeductions,
                NetPay = netPay,
                DeductedAmount = totalBasicDeduction,
                TotalDaysInMonth = totalDaysInPeriod
            };
        }




        private static PayrollResponseDto MapToResponseDto(Payroll payroll)
        {
            return new PayrollResponseDto
            {
                PayrollId = payroll.PayrollId,
                EmployeeId = payroll.EmployeeId,
                EmployeeName = $"{payroll.Employee?.FirstName} {payroll.Employee?.LastName}".Trim(),
                PayrollMonth = payroll.PayrollMonth,
                GrossSalary = payroll.GrossSalary,
                TotalDeductions = payroll.TotalDeductions,
                Bonus = payroll.Bonus,
                NetPay = payroll.NetPay,
                PaymentDate = payroll.PaymentDate,
                PaymentStatus = payroll.PaymentStatus
            };
        }
    }
}