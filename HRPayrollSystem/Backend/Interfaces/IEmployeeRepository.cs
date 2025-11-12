using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(string id);
        Task<IEnumerable<Employee>> GetByDepartmentIdAsync(int departmentId);
        Task<IEnumerable<Employee>> GetByRoleIdAsync(int roleId);
        Task<IEnumerable<Employee>> GetByManagerIdAsync(string managerId);
        Task<IEnumerable<Employee>> SearchAsync(string searchTerm);
        Task<Employee> CreateAsync(Employee employee);
        Task<Employee> UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> OfficeEmailExistsAsync(string officeEmail, string? excludeId = null);
        Task<bool> MobileNumberExistsAsync(string mobileNumber, string? excludeId = null);
        Task<bool> DepartmentExistsAsync(int departmentId);
        Task<bool> RoleExistsAsync(int roleId);
        Task<string> GetNextEmployeeIdAsync();
        Task<Employee?> GetByOfficeEmailAsync(string officeEmail);
        Task<int> GetSubordinatesCountAsync(string employeeId);
    }
}