using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class SalaryStructureRepository : ISalaryStructureRepository
    {
        private readonly HRPayrollDbContext _context;

        public SalaryStructureRepository(HRPayrollDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SalaryStructure>> GetAllAsync()
        {
            return await _context.SalaryStructures
                .Include(s => s.Employee)
                .ToListAsync();
        }

        public async Task<SalaryStructure?> GetByIdAsync(int id)
        {
            return await _context.SalaryStructures
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.SalaryStructureId == id);
        }

        public async Task<SalaryStructure?> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.SalaryStructures
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);
        }

        public async Task<SalaryStructure> CreateAsync(SalaryStructure salaryStructure)
        {
            _context.SalaryStructures.Add(salaryStructure);
            await _context.SaveChangesAsync();
            return salaryStructure;
        }

        public async Task<SalaryStructure> UpdateAsync(SalaryStructure salaryStructure)
        {
            _context.SalaryStructures.Update(salaryStructure);
            await _context.SaveChangesAsync();
            return salaryStructure;
        }



        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.SalaryStructures.AnyAsync(s => s.SalaryStructureId == id);
        }

        public async Task<bool> EmployeeExistsAsync(string employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> EmployeeHasSalaryStructureAsync(string employeeId, int? excludeId = null)
        {
            return await _context.SalaryStructures
                .AnyAsync(s => s.EmployeeId == employeeId 
                          && (excludeId == null || s.SalaryStructureId != excludeId));
        }
    }
}