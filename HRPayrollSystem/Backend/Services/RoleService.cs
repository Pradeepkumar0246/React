using HRPayrollSystem_Payslip.DTOs.RoleDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleResponseDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                DepartmentId = r.DepartmentId,
                DepartmentName = r.Department?.DepartmentName,
                EmployeeCount = r.Employees?.Count ?? 0
            });
        }

        public async Task<RoleResponseDto?> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return null;

            return new RoleResponseDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                DepartmentId = role.DepartmentId,
                DepartmentName = role.Department?.DepartmentName,
                EmployeeCount = role.Employees?.Count ?? 0
            };
        }

        public async Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto roleCreateDto)
        {
            if (await _roleRepository.RoleNameExistsAsync(roleCreateDto.RoleName))
            {
                throw new InvalidOperationException("Role name already exists.");
            }

            if (roleCreateDto.DepartmentId.HasValue && !await _roleRepository.DepartmentExistsAsync(roleCreateDto.DepartmentId.Value))
            {
                throw new KeyNotFoundException("Department not found.");
            }

            var role = new Role
            {
                RoleName = roleCreateDto.RoleName,
                Description = roleCreateDto.Description,
                DepartmentId = roleCreateDto.DepartmentId
            };

            var createdRole = await _roleRepository.CreateAsync(role);

            return new RoleResponseDto
            {
                RoleId = createdRole.RoleId,
                RoleName = createdRole.RoleName,
                Description = createdRole.Description,
                DepartmentId = createdRole.DepartmentId,
                EmployeeCount = 0
            };
        }

        public async Task<RoleResponseDto> UpdateRoleAsync(RoleUpdateDto roleUpdateDto)
        {
            if (!await _roleRepository.ExistsAsync(roleUpdateDto.RoleId))
            {
                throw new KeyNotFoundException("Role not found.");
            }

            if (await _roleRepository.RoleNameExistsAsync(roleUpdateDto.RoleName, roleUpdateDto.RoleId))
            {
                throw new InvalidOperationException("Role name already exists.");
            }

            if (roleUpdateDto.DepartmentId.HasValue && !await _roleRepository.DepartmentExistsAsync(roleUpdateDto.DepartmentId.Value))
            {
                throw new KeyNotFoundException("Department not found.");
            }

            var role = new Role
            {
                RoleId = roleUpdateDto.RoleId,
                RoleName = roleUpdateDto.RoleName,
                Description = roleUpdateDto.Description,
                DepartmentId = roleUpdateDto.DepartmentId
            };

            var updatedRole = await _roleRepository.UpdateAsync(role);
            var employeeCount = await _roleRepository.GetEmployeeCountAsync(updatedRole.RoleId);

            return new RoleResponseDto
            {
                RoleId = updatedRole.RoleId,
                RoleName = updatedRole.RoleName,
                Description = updatedRole.Description,
                DepartmentId = updatedRole.DepartmentId,
                EmployeeCount = employeeCount
            };
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            if (!await _roleRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException("Role not found.");
            }

            var employeeCount = await _roleRepository.GetEmployeeCountAsync(id);
            if (employeeCount > 0)
            {
                throw new InvalidOperationException("Cannot delete role with existing employees.");
            }

            return await _roleRepository.DeleteAsync(id);
        }
    }
}