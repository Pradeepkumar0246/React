using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly HRPayrollDbContext _context;
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(HRPayrollDbContext context, ILogger<EmployeeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .Include(e => e.ReportingManager)
                .Include(e => e.SalaryStructure)
                .OrderBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .Include(e => e.ReportingManager)
                .Include(e => e.SalaryStructure)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .Include(e => e.ReportingManager)
                .Where(e => e.DepartmentId == departmentId)
                .OrderBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByRoleIdAsync(int roleId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .Include(e => e.ReportingManager)
                .Where(e => e.RoleId == roleId)
                .OrderBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByManagerIdAsync(string managerId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .Include(e => e.ReportingManager)
                .Where(e => e.ReportingManagerId == managerId)
                .OrderBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> SearchAsync(string searchTerm)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .Include(e => e.ReportingManager)
                .Where(e => e.FirstName.Contains(searchTerm) ||
                           e.LastName.Contains(searchTerm) ||
                           e.EmployeeId.Contains(searchTerm) ||
                           e.OfficeEmail.Contains(searchTerm) ||
                           e.PersonalEmail.Contains(searchTerm))
                .OrderBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<Employee> CreateAsync(Employee employee)
        {
            try
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Employee {EmployeeId} created in database", employee.EmployeeId);
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error creating employee {EmployeeId}", employee.EmployeeId);
                throw;
            }
        }

        public async Task<Employee> UpdateAsync(Employee employee)
        {
            var existingEntity = _context.Employees.Local.FirstOrDefault(e => e.EmployeeId == employee.EmployeeId);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = EntityState.Detached;
            }
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found in database for deletion", id);
                    return false;
                }

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Employee {EmployeeId} deleted from database", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error deleting employee {EmployeeId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == id);
        }

        public async Task<bool> OfficeEmailExistsAsync(string officeEmail, string? excludeId = null)
        {
            return await _context.Employees
                .AnyAsync(e => e.OfficeEmail.ToLower() == officeEmail.ToLower() 
                          && (excludeId == null || e.EmployeeId != excludeId));
        }

        public async Task<bool> MobileNumberExistsAsync(string mobileNumber, string? excludeId = null)
        {
            return await _context.Employees
                .AnyAsync(e => e.MobileNumber == mobileNumber 
                          && (excludeId == null || e.EmployeeId != excludeId));
        }

        public async Task<bool> DepartmentExistsAsync(int departmentId)
        {
            return await _context.Departments.AnyAsync(d => d.DepartmentId == departmentId);
        }

        public async Task<bool> RoleExistsAsync(int roleId)
        {
            return await _context.Roles.AnyAsync(r => r.RoleId == roleId);
        }

        public async Task<string> GetNextEmployeeIdAsync()
        {
            var lastEmployee = await _context.Employees
                .Where(e => e.EmployeeId.StartsWith("EMP"))
                .OrderByDescending(e => e.EmployeeId)
                .FirstOrDefaultAsync();

            if (lastEmployee == null)
            {
                return "EMP001";
            }

            var lastNumber = int.Parse(lastEmployee.EmployeeId.Substring(3));
            return $"EMP{(lastNumber + 1):D3}";
        }

        public async Task<Employee?> GetByOfficeEmailAsync(string officeEmail)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.OfficeEmail.ToLower() == officeEmail.ToLower());
        }

        public async Task<int> GetSubordinatesCountAsync(string employeeId)
        {
            return await _context.Employees
                .CountAsync(e => e.ReportingManagerId == employeeId);
        }
    }
}