using HRPayrollSystem_Payslip.DTOs.PayrollDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;

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

            // Calculate working days and attendance
            var monthStart = month;
            var monthEnd = month.AddMonths(1).AddDays(-1);
            var totalWorkingDays = CalculateWorkingDays(monthStart, monthEnd);
            
            // Get real attendance and leave data
            var realAttendedDays = await CalculateRealAttendedDaysAsync(payrollGenerateDto.EmployeeId, monthStart, monthEnd);
            var realPaidLeaveDays = await CalculateRealPaidLeaveDaysAsync(payrollGenerateDto.EmployeeId, monthStart, monthEnd);
            var realLOPDays = await CalculateRealLOPDaysAsync(payrollGenerateDto.EmployeeId, monthStart, monthEnd);
            
            // Use dummy values for attendance calculation (as requested)
            var dummyAttendedDays = totalWorkingDays; // Full attendance
            var dummyPaidLeaveDays = 0; // No paid leave impact
            
            // Calculate effective working days using dummy values but real LOP
            var effectiveWorkingDays = dummyAttendedDays + dummyPaidLeaveDays - realLOPDays;
            var salaryRatio = totalWorkingDays > 0 ? effectiveWorkingDays / totalWorkingDays : 1;
            
            // Calculate salary components
            var grossSalary = salaryStructure.BasicSalary + salaryStructure.HRA + salaryStructure.Allowances;
            var adjustedGrossSalary = grossSalary * salaryRatio;
            var bonus = payrollGenerateDto.BonusAmount ?? 0;
            var totalDeductions = salaryStructure.Deductions + salaryStructure.PF + salaryStructure.Tax + (payrollGenerateDto.AdditionalDeductions ?? 0);
            var netPay = adjustedGrossSalary + bonus - totalDeductions;

            var payroll = new Payroll
            {
                EmployeeId = payrollGenerateDto.EmployeeId,
                PayrollMonth = month,
                GrossSalary = adjustedGrossSalary,
                TotalDeductions = totalDeductions,
                Bonus = bonus,
                NetPay = netPay,
                PaymentDate = DateTime.Today.AddDays(5),
                PaymentStatus = payrollGenerateDto.PaymentStatus
            };

            var createdPayroll = await _payrollRepository.CreateAsync(payroll);
            
            // Auto-generate payslip
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
            
                var result = await _payrollRepository.GetByIdAsync(createdPayroll.PayrollId);
                stopwatch.Stop();
                _logger.LogInformation("Payroll generated for employee {EmployeeId} with net pay {NetPay} in {ElapsedMs}ms", payrollGenerateDto.EmployeeId, netPay, stopwatch.ElapsedMilliseconds);
                return MapToResponseDto(result!);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to generate payroll for employee {EmployeeId} after {ElapsedMs}ms", payrollGenerateDto.EmployeeId, stopwatch.ElapsedMilliseconds);
                throw;
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

        private async Task<int> CalculateRealAttendedDaysAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            // Count actual attendance days from database
            var attendanceRecords = await _payrollRepository.GetAttendanceRecordsAsync(employeeId, startDate, endDate);
            return attendanceRecords.Count(a => a.Status == Enums.AttendanceStatus.Present);
        }

        private async Task<decimal> CalculateRealPaidLeaveDaysAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            // Count approved paid leave days (CL, SL, EL, Maternity, Paternity)
            var leaveRecords = await _payrollRepository.GetLeaveRecordsAsync(employeeId, startDate, endDate);
            return leaveRecords
                .Where(l => l.Status == Enums.LeaveStatus.Approved && 
                           (l.LeaveType == Enums.LeaveType.Casual || 
                            l.LeaveType == Enums.LeaveType.Sick || 
                            l.LeaveType == Enums.LeaveType.Earned ||
                            l.LeaveType == Enums.LeaveType.Maternity ||
                            l.LeaveType == Enums.LeaveType.Paternity))
                .Sum(l => l.NumberOfDays);
        }

        private async Task<decimal> CalculateRealLOPDaysAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            // Count approved LOP and Emergency leave days
            var leaveRecords = await _payrollRepository.GetLeaveRecordsAsync(employeeId, startDate, endDate);
            return leaveRecords
                .Where(l => l.Status == Enums.LeaveStatus.Approved && 
                           (l.LeaveType == Enums.LeaveType.LOP || 
                            l.LeaveType == Enums.LeaveType.Emergency))
                .Sum(l => l.NumberOfDays);
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