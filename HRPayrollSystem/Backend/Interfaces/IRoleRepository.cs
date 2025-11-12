using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role> CreateAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> RoleNameExistsAsync(string roleName, int? excludeId = null);
        Task<int> GetEmployeeCountAsync(int roleId);
        Task<bool> DepartmentExistsAsync(int departmentId);
    }
}