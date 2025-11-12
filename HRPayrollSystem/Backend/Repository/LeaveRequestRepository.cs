using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using HRPayrollSystem_Payslip.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly HRPayrollDbContext _context;

        public LeaveRequestRepository(HRPayrollDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.Approver)
                .OrderByDescending(l => l.FromDate)
                .ToListAsync();
        }

        public async Task<LeaveRequest?> GetByIdAsync(int id)
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.Approver)
                .FirstOrDefaultAsync(l => l.LeaveRequestId == id);
        }

        public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.Approver)
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.FromDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status)
        {
            if (Enum.TryParse<LeaveStatus>(status, true, out var leaveStatus))
            {
                return await _context.LeaveRequests
                    .Include(l => l.Employee)
                    .Include(l => l.Approver)
                    .Where(l => l.Status == leaveStatus)
                    .OrderByDescending(l => l.FromDate)
                    .ToListAsync();
            }
            return new List<LeaveRequest>();
        }

        public async Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();
            return leaveRequest;
        }

        public async Task<LeaveRequest> UpdateAsync(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Update(leaveRequest);
            await _context.SaveChangesAsync();
            return leaveRequest;
        }



        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.LeaveRequests.AnyAsync(l => l.LeaveRequestId == id);
        }

        public async Task<bool> EmployeeExistsAsync(string employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> HasOverlappingLeaveAsync(string employeeId, DateTime fromDate, DateTime toDate, int? excludeId = null)
        {
            return await _context.LeaveRequests
                .AnyAsync(l => l.EmployeeId == employeeId
                          && l.Status == LeaveStatus.Approved
                          && l.FromDate <= toDate
                          && l.ToDate >= fromDate
                          && (excludeId == null || l.LeaveRequestId != excludeId));
        }
    }
}