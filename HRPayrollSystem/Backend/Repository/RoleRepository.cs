using HRPayrollSystem_Payslip.Data;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using Microsoft.EntityFrameworkCore;

namespace HRPayrollSystem_Payslip.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly HRPayrollDbContext _context;

        public RoleRepository(HRPayrollDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles
                .Include(r => r.Department)
                .Include(r => r.Employees)
                .ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.Department)
                .Include(r => r.Employees)
                .FirstOrDefaultAsync(r => r.RoleId == id);
        }

        public async Task<Role> CreateAsync(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role> UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Roles.AnyAsync(r => r.RoleId == id);
        }

        public async Task<bool> RoleNameExistsAsync(string roleName, int? excludeId = null)
        {
            return await _context.Roles
                .AnyAsync(r => r.RoleName.ToLower() == roleName.ToLower() 
                          && (excludeId == null || r.RoleId != excludeId));
        }

        public async Task<int> GetEmployeeCountAsync(int roleId)
        {
            return await _context.Employees
                .CountAsync(e => e.RoleId == roleId);
        }

        public async Task<bool> DepartmentExistsAsync(int departmentId)
        {
            return await _context.Departments.AnyAsync(d => d.DepartmentId == departmentId);
        }
    }
}