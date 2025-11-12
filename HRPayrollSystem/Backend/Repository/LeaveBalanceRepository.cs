using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Enums;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class LeaveBalanceRepository : ILeaveBalanceRepository
    {
        private readonly HRPayrollDbContext _context;

        public LeaveBalanceRepository(HRPayrollDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaveBalance>> GetByEmployeeIdAsync(string employeeId, int year)
        {
            return await _context.LeaveBalances
                .Include(lb => lb.Employee)
                .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
                .ToListAsync();
        }

        public async Task<LeaveBalance?> GetByEmployeeAndLeaveTypeAsync(string employeeId, LeaveType leaveType, int year)
        {
            return await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveType == leaveType && lb.Year == year);
        }

        public async Task<LeaveBalance> CreateAsync(LeaveBalance leaveBalance)
        {
            _context.LeaveBalances.Add(leaveBalance);
            await _context.SaveChangesAsync();
            return leaveBalance;
        }

        public async Task<LeaveBalance> UpdateAsync(LeaveBalance leaveBalance)
        {
            _context.LeaveBalances.Update(leaveBalance);
            await _context.SaveChangesAsync();
            return leaveBalance;
        }

        public async Task<bool> ExistsAsync(string employeeId, LeaveType leaveType, int year)
        {
            return await _context.LeaveBalances
                .AnyAsync(lb => lb.EmployeeId == employeeId && lb.LeaveType == leaveType && lb.Year == year);
        }

        public async Task CreateDefaultBalancesAsync(string employeeId, int year)
        {
            var defaultBalances = new[]
            {
                new LeaveBalance { EmployeeId = employeeId, LeaveType = LeaveType.Casual, AllocatedDays = 12, RemainingDays = 12, Year = year },
                new LeaveBalance { EmployeeId = employeeId, LeaveType = LeaveType.Sick, AllocatedDays = 12, RemainingDays = 12, Year = year },
                new LeaveBalance { EmployeeId = employeeId, LeaveType = LeaveType.Earned, AllocatedDays = 21, RemainingDays = 21, Year = year }
            };

            _context.LeaveBalances.AddRange(defaultBalances);
            await _context.SaveChangesAsync();
        }

        public async Task ProcessYearEndAsync(int year)
        {
            var nextYear = year + 1;
            var employees = await _context.Employees.Select(e => e.EmployeeId).ToListAsync();

            foreach (var employeeId in employees)
            {
                // Get current year EL balance
                var elBalance = await GetByEmployeeAndLeaveTypeAsync(employeeId, LeaveType.Earned, year);
                var carryForward = elBalance?.RemainingDays > 5 ? 5 : (elBalance?.RemainingDays ?? 0);

                // Create next year balances
                var nextYearBalances = new[]
                {
                    new LeaveBalance { EmployeeId = employeeId, LeaveType = LeaveType.Casual, AllocatedDays = 12, RemainingDays = 12, Year = nextYear },
                    new LeaveBalance { EmployeeId = employeeId, LeaveType = LeaveType.Sick, AllocatedDays = 12, RemainingDays = 12, Year = nextYear },
                    new LeaveBalance { EmployeeId = employeeId, LeaveType = LeaveType.Earned, AllocatedDays = 21 + carryForward, RemainingDays = 21 + carryForward, Year = nextYear }
                };

                _context.LeaveBalances.AddRange(nextYearBalances);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Employee?> GetEmployeeAsync(string employeeId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }
    }
}