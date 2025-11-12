using HRPayrollSystem_Payslip.DTOs.DepartmentDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetAllDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            return departments.Select(d => new DepartmentResponseDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                EmployeeCount = d.Employees?.Count ?? 0
            });
        }

        public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return null;

            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                EmployeeCount = department.Employees?.Count ?? 0
            };
        }

        public async Task<DepartmentResponseDto> CreateDepartmentAsync(DepartmentCreateDto departmentCreateDto)
        {
            // Business logic: Check if department name already exists
            if (await _departmentRepository.DepartmentNameExistsAsync(departmentCreateDto.DepartmentName))
            {
                throw new InvalidOperationException("Department name already exists.");
            }

            var department = new Department
            {
                DepartmentName = departmentCreateDto.DepartmentName,
                Description = departmentCreateDto.Description
            };

            var createdDepartment = await _departmentRepository.CreateAsync(department);

            return new DepartmentResponseDto
            {
                DepartmentId = createdDepartment.DepartmentId,
                DepartmentName = createdDepartment.DepartmentName,
                Description = createdDepartment.Description,
                EmployeeCount = 0
            };
        }

        public async Task<DepartmentResponseDto> UpdateDepartmentAsync(DepartmentUpdateDto departmentUpdateDto)
        {
            // Business logic: Check if department exists
            if (!await _departmentRepository.ExistsAsync(departmentUpdateDto.DepartmentId))
            {
                throw new KeyNotFoundException("Department not found.");
            }

            // Business logic: Check if department name already exists (excluding current department)
            if (await _departmentRepository.DepartmentNameExistsAsync(departmentUpdateDto.DepartmentName, departmentUpdateDto.DepartmentId))
            {
                throw new InvalidOperationException("Department name already exists.");
            }

            var department = new Department
            {
                DepartmentId = departmentUpdateDto.DepartmentId,
                DepartmentName = departmentUpdateDto.DepartmentName,
                Description = departmentUpdateDto.Description
            };

            var updatedDepartment = await _departmentRepository.UpdateAsync(department);
            var employeeCount = await _departmentRepository.GetEmployeeCountAsync(updatedDepartment.DepartmentId);

            return new DepartmentResponseDto
            {
                DepartmentId = updatedDepartment.DepartmentId,
                DepartmentName = updatedDepartment.DepartmentName,
                Description = updatedDepartment.Description,
                EmployeeCount = employeeCount
            };
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            // Business logic: Check if department exists
            if (!await _departmentRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException("Department not found.");
            }

            // Business logic: Check if department has employees
            var employeeCount = await _departmentRepository.GetEmployeeCountAsync(id);
            if (employeeCount > 0)
            {
                throw new InvalidOperationException("Cannot delete department with existing employees.");
            }

            return await _departmentRepository.DeleteAsync(id);
        }
    }
}