using HRPayrollSystem_Payslip.DTOs.DepartmentDTO;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentResponseDto>> GetAllDepartmentsAsync();
        Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentResponseDto> CreateDepartmentAsync(DepartmentCreateDto departmentCreateDto);
        Task<DepartmentResponseDto> UpdateDepartmentAsync(DepartmentUpdateDto departmentUpdateDto);
        Task<bool> DeleteDepartmentAsync(int id);
    }
}