using HRPayrollSystem_Payslip.DTOs.RoleDTO;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync();
        Task<RoleResponseDto?> GetRoleByIdAsync(int id);
        Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto roleCreateDto);
        Task<RoleResponseDto> UpdateRoleAsync(RoleUpdateDto roleUpdateDto);
        Task<bool> DeleteRoleAsync(int id);
    }
}