using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly HRPayrollDbContext _context;

        public AttendanceRepository(HRPayrollDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Attendance?> GetByIdAsync(int id)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AttendanceId == id);
        }

        public async Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date >= fromDate && a.Date <= toDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Attendance> CreateAsync(Attendance attendance)
        {
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return attendance;
        }

        public async Task<Attendance> UpdateAsync(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
            return attendance;
        }



        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Attendances.AnyAsync(a => a.AttendanceId == id);
        }

        public async Task<bool> EmployeeExistsAsync(string employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> AttendanceExistsForDateAsync(string employeeId, DateTime date, int? excludeId = null)
        {
            return await _context.Attendances
                .AnyAsync(a => a.EmployeeId == employeeId 
                          && a.Date.Date == date.Date 
                          && (excludeId == null || a.AttendanceId != excludeId));
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(string employeeId, DateTime date)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId && a.Date.Date == date.Date)
                .OrderByDescending(a => a.AttendanceId)
                .FirstOrDefaultAsync();
        }
    }
}