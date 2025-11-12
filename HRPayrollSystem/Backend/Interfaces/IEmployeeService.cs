using HRPayrollSystem_Payslip.DTOs.EmployeeDTO;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync();
        Task<EmployeeResponseDto?> GetEmployeeByIdAsync(string id);
        Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByDepartmentIdAsync(int departmentId);
        Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByRoleIdAsync(int roleId);
        Task<IEnumerable<EmployeeResponseDto>> GetSubordinatesAsync(string managerId);
        Task<IEnumerable<EmployeeResponseDto>> SearchEmployeesAsync(string searchTerm);
        Task<EmployeeResponseDto> CreateEmployeeAsync(EmployeeCreateDto employeeCreateDto);
        Task<EmployeeResponseDto> UpdateEmployeeAsync(EmployeeUpdateDto employeeUpdateDto);
        Task<bool> DeleteEmployeeAsync(string id);
        Task<bool> ChangePasswordAsync(EmployeePasswordChangeDto passwordChangeDto);
        Task<bool> ActivateEmployeeAsync(string id);
        Task<bool> DeactivateEmployeeAsync(string id);
    }
}