using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class PayslipRepository : IPayslipRepository
    {
        private readonly HRPayrollDbContext _context;

        public PayslipRepository(HRPayrollDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payslip>> GetAllAsync()
        {
            return await _context.Payslips
                .Include(p => p.Employee)
                .Include(p => p.Payroll)
                .OrderByDescending(p => p.GeneratedDate)
                .ToListAsync();
        }

        public async Task<Payslip?> GetByIdAsync(int id)
        {
            return await _context.Payslips
                .Include(p => p.Employee)
                .Include(p => p.Payroll)
                .FirstOrDefaultAsync(p => p.PayslipId == id);
        }

        public async Task<IEnumerable<Payslip>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.Payslips
                .Include(p => p.Employee)
                .Include(p => p.Payroll)
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.GeneratedDate)
                .ToListAsync();
        }

        public async Task<Payslip?> GetByPayrollIdAsync(int payrollId)
        {
            return await _context.Payslips
                .Include(p => p.Employee)
                .Include(p => p.Payroll)
                .FirstOrDefaultAsync(p => p.PayrollId == payrollId);
        }



        public async Task<Payslip> UpdateAsync(Payslip payslip)
        {
            _context.Payslips.Update(payslip);
            await _context.SaveChangesAsync();
            return payslip;
        }



        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Payslips.AnyAsync(p => p.PayslipId == id);
        }

        public async Task<bool> EmployeeExistsAsync(string employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> PayrollExistsAsync(int payrollId)
        {
            return await _context.Payrolls.AnyAsync(p => p.PayrollId == payrollId);
        }

        public async Task<bool> PayslipExistsForPayrollAsync(int payrollId, int? excludeId = null)
        {
            return await _context.Payslips
                .AnyAsync(p => p.PayrollId == payrollId 
                          && (excludeId == null || p.PayslipId != excludeId));
        }

        public async Task<Payroll?> GetPayrollByIdAsync(int payrollId)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.PayrollId == payrollId);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(string employeeId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }
    }
}