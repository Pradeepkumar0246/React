using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using HRPayrollSystem_Payslip.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class EmployeeDocumentRepository : IEmployeeDocumentRepository
    {
        private readonly HRPayrollDbContext _context;

        public EmployeeDocumentRepository(HRPayrollDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeDocument>> GetAllAsync()
        {
            return await _context.EmployeeDocuments
                .Include(d => d.Employee)
                .OrderByDescending(d => d.UploadedDate)
                .ToListAsync();
        }

        public async Task<EmployeeDocument?> GetByIdAsync(int id)
        {
            return await _context.EmployeeDocuments
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.DocumentId == id);
        }

        public async Task<IEnumerable<EmployeeDocument>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.EmployeeDocuments
                .Include(d => d.Employee)
                .Where(d => d.EmployeeId == employeeId)
                .OrderByDescending(d => d.UploadedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeDocument>> GetByCategoryAsync(string category)
        {
            if (Enum.TryParse<DocumentCategory>(category, true, out var documentCategory))
            {
                return await _context.EmployeeDocuments
                    .Include(d => d.Employee)
                    .Where(d => d.Category == documentCategory)
                    .OrderByDescending(d => d.UploadedDate)
                    .ToListAsync();
            }
            return new List<EmployeeDocument>();
        }

        public async Task<EmployeeDocument> CreateAsync(EmployeeDocument employeeDocument)
        {
            _context.EmployeeDocuments.Add(employeeDocument);
            await _context.SaveChangesAsync();
            return employeeDocument;
        }

        public async Task<EmployeeDocument> UpdateAsync(EmployeeDocument employeeDocument)
        {
            _context.EmployeeDocuments.Update(employeeDocument);
            await _context.SaveChangesAsync();
            return employeeDocument;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document = await _context.EmployeeDocuments.FindAsync(id);
            if (document == null) return false;

            _context.EmployeeDocuments.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.EmployeeDocuments.AnyAsync(d => d.DocumentId == id);
        }

        public async Task<bool> EmployeeExistsAsync(string employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }
    }
}