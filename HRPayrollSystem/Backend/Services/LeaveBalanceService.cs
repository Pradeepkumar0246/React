using HRPayrollSystem_Payslip.DTOs.LeaveBalanceDTO;
using HRPayrollSystem_Payslip.Enums;
using HRPayrollSystem_Payslip.Interfaces;

namespace HRPayrollSystem_Payslip.Services
{
    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly ILeaveBalanceRepository _leaveBalanceRepository;

        public LeaveBalanceService(ILeaveBalanceRepository leaveBalanceRepository)
        {
            _leaveBalanceRepository = leaveBalanceRepository;
        }

        public async Task<IEnumerable<LeaveBalanceResponseDto>> GetEmployeeLeaveBalancesAsync(string employeeId, int? year = null)
        {
            var currentYear = year ?? DateTime.Now.Year;
            
            // Check employee joining year
            var employee = await _leaveBalanceRepository.GetEmployeeAsync(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee {employeeId} not found.");
            }
            
            var joiningYear = employee.DateOfJoining.Year;
            if (currentYear < joiningYear)
            {
                throw new InvalidOperationException($"Employee {employeeId} joined in {joiningYear}. Cannot get leave balance for {currentYear}.");
            }
            
            var balances = await _leaveBalanceRepository.GetByEmployeeIdAsync(employeeId, currentYear);
            
            if (!balances.Any())
            {
                throw new KeyNotFoundException($"No leave balance found for employee {employeeId} in year {currentYear}.");
            }
            
            return balances.Select(b => new LeaveBalanceResponseDto
            {
                LeaveBalanceId = b.LeaveBalanceId,
                EmployeeId = b.EmployeeId,
                EmployeeName = $"{b.Employee?.FirstName} {b.Employee?.LastName}".Trim(),
                LeaveType = b.LeaveType,
                AllocatedDays = b.AllocatedDays,
                UsedDays = b.UsedDays,
                RemainingDays = b.RemainingDays,
                Year = b.Year
            });
        }

        public async Task<bool> HasSufficientBalanceAsync(string employeeId, LeaveType leaveType, decimal requestedDays, int year)
        {
            // Emergency and LOP don't need balance check
            if (leaveType == LeaveType.Emergency || leaveType == LeaveType.LOP)
                return true;

            var balance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(employeeId, leaveType, year);
            return balance != null && balance.RemainingDays >= requestedDays;
        }

        public async Task UpdateLeaveBalanceAsync(string employeeId, LeaveType leaveType, decimal usedDays, int year)
        {
            // Skip balance update for Emergency and LOP
            if (leaveType == LeaveType.Emergency || leaveType == LeaveType.LOP)
                return;

            var balance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(employeeId, leaveType, year);
            if (balance != null)
            {
                balance.UsedDays += usedDays;
                balance.RemainingDays = balance.AllocatedDays - balance.UsedDays;
                await _leaveBalanceRepository.UpdateAsync(balance);
            }
        }

        public async Task CreateDefaultBalancesAsync(string employeeId)
        {
            var currentYear = DateTime.Now.Year;
            await _leaveBalanceRepository.CreateDefaultBalancesAsync(employeeId, currentYear);
        }

        public async Task ProcessYearEndAsync()
        {
            var currentYear = DateTime.Now.Year;
            await _leaveBalanceRepository.ProcessYearEndAsync(currentYear);
        }
    }
}