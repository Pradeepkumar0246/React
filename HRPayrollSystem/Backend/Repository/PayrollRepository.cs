using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class PayrollRepository : IPayrollRepository
    {
        private readonly HRPayrollDbContext _context;
        private readonly ILogger<PayrollRepository> _logger;

        public PayrollRepository(HRPayrollDbContext context, ILogger<PayrollRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Payroll>> GetAllAsync()
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .OrderByDescending(p => p.PayrollMonth)
                .ToListAsync();
        }

        public async Task<Payroll?> GetByIdAsync(int id)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.PayrollId == id);
        }

        public async Task<IEnumerable<Payroll>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.PayrollMonth)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payroll>> GetByMonthAsync(DateTime month)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.PayrollMonth.Year == month.Year && p.PayrollMonth.Month == month.Month)
                .OrderBy(p => p.Employee!.FirstName)
                .ToListAsync();
        }

        public async Task<Payroll> CreateAsync(Payroll payroll)
        {
            try
            {
                _context.Payrolls.Add(payroll);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Payroll {PayrollId} created in database for employee {EmployeeId}", payroll.PayrollId, payroll.EmployeeId);
                return payroll;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error creating payroll for employee {EmployeeId}", payroll.EmployeeId);
                throw;
            }
        }

        public async Task<Payroll> UpdateAsync(Payroll payroll)
        {
            _context.Payrolls.Update(payroll);
            await _context.SaveChangesAsync();
            return payroll;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll == null) return false;

            // Delete related payslips first
            var relatedPayslips = await _context.Payslips.Where(p => p.PayrollId == id).ToListAsync();
            _context.Payslips.RemoveRange(relatedPayslips);
            
            // Then delete payroll
            _context.Payrolls.Remove(payroll);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Payrolls.AnyAsync(p => p.PayrollId == id);
        }

        public async Task<bool> EmployeeExistsAsync(string employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> PayrollExistsForMonthAsync(string employeeId, DateTime month, int? excludeId = null)
        {
            return await _context.Payrolls
                .AnyAsync(p => p.EmployeeId == employeeId
                          && p.PayrollMonth.Year == month.Year
                          && p.PayrollMonth.Month == month.Month
                          && (excludeId == null || p.PayrollId != excludeId));
        }

        public async Task<SalaryStructure?> GetEmployeeSalaryStructureAsync(string employeeId)
        {
            return await _context.SalaryStructures
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);
        }

        public async Task<Payslip> CreatePayslipAsync(Payslip payslip)
        {
            _context.Payslips.Add(payslip);
            await _context.SaveChangesAsync();
            return payslip;
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceRecordsAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && 
                           a.Date >= startDate.Date && 
                           a.Date <= endDate.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRecordsAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.LeaveRequests
                .Where(l => l.EmployeeId == employeeId && 
                           l.FromDate >= startDate.Date && 
                           l.ToDate <= endDate.Date)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(string employeeId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }
    }
}