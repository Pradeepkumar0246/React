using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using HRPayrollSystem_Payslip.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly HRPayrollDbContext _context;

        public AuditLogRepository(HRPayrollDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _context.AuditLogs
                .FirstOrDefaultAsync(a => a.AuditId == id);
        }

        public async Task<IEnumerable<AuditLog>> GetByActionTypeAsync(string actionType)
        {
            if (Enum.TryParse<ActionType>(actionType, true, out var enumActionType))
            {
                return await _context.AuditLogs
                    .Where(a => a.Action == enumActionType)
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();
            }
            return new List<AuditLog>();
        }

        public async Task<IEnumerable<AuditLog>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.AuditLogs
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.AuditLogs
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }







        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.AuditLogs.AnyAsync(a => a.AuditId == id);
        }
    }
}