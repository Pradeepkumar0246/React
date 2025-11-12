using HRPayrollSystem_Payslip.DTOs.EmployeeDTO;
using HRPayrollSystem_Payslip.Enums;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using System.Security.Cryptography;
using System.Text;

namespace HRPayrollSystem_Payslip.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILeaveBalanceService _leaveBalanceService;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IEmployeeRepository employeeRepository, IWebHostEnvironment environment, ILeaveBalanceService leaveBalanceService, ILogger<EmployeeService> logger)
        {
            _employeeRepository = employeeRepository;
            _environment = environment;
            _leaveBalanceService = leaveBalanceService;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            var responseDtos = new List<EmployeeResponseDto>();

            foreach (var employee in employees)
            {
                var subordinatesCount = await _employeeRepository.GetSubordinatesCountAsync(employee.EmployeeId);
                responseDtos.Add(await MapToResponseDtoAsync(employee, subordinatesCount));
            }

            return responseDtos;
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(string id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return null;

            var subordinatesCount = await _employeeRepository.GetSubordinatesCountAsync(employee.EmployeeId);
            return await MapToResponseDtoAsync(employee, subordinatesCount);
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            if (!await _employeeRepository.DepartmentExistsAsync(departmentId))
            {
                throw new KeyNotFoundException("Department not found.");
            }

            var employees = await _employeeRepository.GetByDepartmentIdAsync(departmentId);
            var responseDtos = new List<EmployeeResponseDto>();

            foreach (var employee in employees)
            {
                var subordinatesCount = await _employeeRepository.GetSubordinatesCountAsync(employee.EmployeeId);
                responseDtos.Add(await MapToResponseDtoAsync(employee, subordinatesCount));
            }

            return responseDtos;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByRoleIdAsync(int roleId)
        {
            if (!await _employeeRepository.RoleExistsAsync(roleId))
            {
                throw new KeyNotFoundException("Role not found.");
            }

            var employees = await _employeeRepository.GetByRoleIdAsync(roleId);
            var responseDtos = new List<EmployeeResponseDto>();

            foreach (var employee in employees)
            {
                var subordinatesCount = await _employeeRepository.GetSubordinatesCountAsync(employee.EmployeeId);
                responseDtos.Add(await MapToResponseDtoAsync(employee, subordinatesCount));
            }

            return responseDtos;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetSubordinatesAsync(string managerId)
        {
            if (!await _employeeRepository.ExistsAsync(managerId))
            {
                throw new KeyNotFoundException("Manager not found.");
            }

            var employees = await _employeeRepository.GetByManagerIdAsync(managerId);
            var responseDtos = new List<EmployeeResponseDto>();

            foreach (var employee in employees)
            {
                var subordinatesCount = await _employeeRepository.GetSubordinatesCountAsync(employee.EmployeeId);
                responseDtos.Add(await MapToResponseDtoAsync(employee, subordinatesCount));
            }

            return responseDtos;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> SearchEmployeesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term cannot be empty.");
            }

            var employees = await _employeeRepository.SearchAsync(searchTerm);
            var responseDtos = new List<EmployeeResponseDto>();

            foreach (var employee in employees)
            {
                var subordinatesCount = await _employeeRepository.GetSubordinatesCountAsync(employee.EmployeeId);
                responseDtos.Add(await MapToResponseDtoAsync(employee, subordinatesCount));
            }

            return responseDtos;
        }

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(EmployeeCreateDto employeeCreateDto)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (!await _employeeRepository.DepartmentExistsAsync(employeeCreateDto.DepartmentId))
                {
                    _logger.LogError("Department {DepartmentId} not found during employee creation", employeeCreateDto.DepartmentId);
                    throw new KeyNotFoundException("Department not found.");
                }

                if (!await _employeeRepository.RoleExistsAsync(employeeCreateDto.RoleId))
                {
                    _logger.LogError("Role {RoleId} not found during employee creation", employeeCreateDto.RoleId);
                    throw new KeyNotFoundException("Role not found.");
                }

                if (await _employeeRepository.OfficeEmailExistsAsync(employeeCreateDto.OfficeEmail))
                {
                    _logger.LogError("Office email already exists during employee creation");
                    throw new InvalidOperationException("Office email already exists.");
                }

            if (await _employeeRepository.MobileNumberExistsAsync(employeeCreateDto.MobileNumber))
            {
                throw new InvalidOperationException("Mobile number already exists.");
            }

            if (!string.IsNullOrEmpty(employeeCreateDto.ReportingManagerId) && 
                !await _employeeRepository.ExistsAsync(employeeCreateDto.ReportingManagerId))
            {
                throw new KeyNotFoundException("Reporting manager not found.");
            }

            if (employeeCreateDto.DateOfJoining > DateTime.Today)
            {
                throw new ArgumentException("Date of joining cannot be in the future.");
            }

            var employeeId = await _employeeRepository.GetNextEmployeeIdAsync();
            var profilePicturePath = await SaveProfilePictureAsync(employeeCreateDto.ProfilePicture, employeeId);

            var employee = new Employee
            {
                EmployeeId = employeeId,
                FirstName = employeeCreateDto.FirstName,
                LastName = employeeCreateDto.LastName,
                ProfilePicture = profilePicturePath,
                MobileNumber = employeeCreateDto.MobileNumber,
                AlternateNumber = employeeCreateDto.AlternateNumber,
                PersonalEmail = employeeCreateDto.PersonalEmail,
                OfficeEmail = employeeCreateDto.OfficeEmail,
                PasswordHash = HashPassword(employeeCreateDto.Password),
                DepartmentId = employeeCreateDto.DepartmentId,
                RoleId = employeeCreateDto.RoleId,
                EmploymentType = employeeCreateDto.EmploymentType,
                DateOfJoining = employeeCreateDto.DateOfJoining,
                ReportingManagerId = employeeCreateDto.ReportingManagerId,
                Status = EmployeeStatus.Active
            };

            var createdEmployee = await _employeeRepository.CreateAsync(employee);
            
                // Auto-create leave balances for new employee
                await _leaveBalanceService.CreateDefaultBalancesAsync(createdEmployee.EmployeeId);
                
                var result = await _employeeRepository.GetByIdAsync(createdEmployee.EmployeeId);
                stopwatch.Stop();
                _logger.LogInformation("Employee {EmployeeId} created successfully in {ElapsedMs}ms", createdEmployee.EmployeeId, stopwatch.ElapsedMilliseconds);
                return await MapToResponseDtoAsync(result!, 0);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to create employee after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<EmployeeResponseDto> UpdateEmployeeAsync(EmployeeUpdateDto employeeUpdateDto)
        {
            if (!await _employeeRepository.ExistsAsync(employeeUpdateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (!await _employeeRepository.DepartmentExistsAsync(employeeUpdateDto.DepartmentId))
            {
                throw new KeyNotFoundException("Department not found.");
            }

            if (!await _employeeRepository.RoleExistsAsync(employeeUpdateDto.RoleId))
            {
                throw new KeyNotFoundException("Role not found.");
            }

            if (await _employeeRepository.OfficeEmailExistsAsync(employeeUpdateDto.OfficeEmail, employeeUpdateDto.EmployeeId))
            {
                throw new InvalidOperationException("Office email already exists.");
            }

            if (await _employeeRepository.MobileNumberExistsAsync(employeeUpdateDto.MobileNumber, employeeUpdateDto.EmployeeId))
            {
                throw new InvalidOperationException("Mobile number already exists.");
            }

            if (!string.IsNullOrEmpty(employeeUpdateDto.ReportingManagerId) && 
                !await _employeeRepository.ExistsAsync(employeeUpdateDto.ReportingManagerId))
            {
                throw new KeyNotFoundException("Reporting manager not found.");
            }

            if (employeeUpdateDto.DateOfJoining > DateTime.Today)
            {
                throw new ArgumentException("Date of joining cannot be in the future.");
            }

            var existingEmployee = await _employeeRepository.GetByIdAsync(employeeUpdateDto.EmployeeId);
            var profilePicturePath = existingEmployee!.ProfilePicture;

            if (employeeUpdateDto.ProfilePicture != null)
            {
                DeleteProfilePicture(profilePicturePath);
                profilePicturePath = await SaveProfilePictureAsync(employeeUpdateDto.ProfilePicture, employeeUpdateDto.EmployeeId);
            }

            var employee = new Employee
            {
                EmployeeId = employeeUpdateDto.EmployeeId,
                FirstName = employeeUpdateDto.FirstName,
                LastName = employeeUpdateDto.LastName,
                ProfilePicture = profilePicturePath,
                MobileNumber = employeeUpdateDto.MobileNumber,
                AlternateNumber = employeeUpdateDto.AlternateNumber,
                PersonalEmail = employeeUpdateDto.PersonalEmail,
                OfficeEmail = employeeUpdateDto.OfficeEmail,
                PasswordHash = existingEmployee.PasswordHash,
                DepartmentId = employeeUpdateDto.DepartmentId,
                RoleId = employeeUpdateDto.RoleId,
                EmploymentType = employeeUpdateDto.EmploymentType,
                DateOfJoining = employeeUpdateDto.DateOfJoining,
                ReportingManagerId = employeeUpdateDto.ReportingManagerId,
                Status = employeeUpdateDto.Status
            };

            var updatedEmployee = await _employeeRepository.UpdateAsync(employee);
            var result = await _employeeRepository.GetByIdAsync(updatedEmployee.EmployeeId);
            var subordinatesCount = await _employeeRepository.GetSubordinatesCountAsync(result!.EmployeeId);
            return await MapToResponseDtoAsync(result, subordinatesCount);
        }

        public async Task<bool> DeleteEmployeeAsync(string id)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (!await _employeeRepository.ExistsAsync(id))
                {
                    _logger.LogError("Employee {EmployeeId} not found for deletion", id);
                    throw new KeyNotFoundException("Employee not found.");
                }

                var subordinatesCount = await _employeeRepository.GetSubordinatesCountAsync(id);
                if (subordinatesCount > 0)
                {
                    _logger.LogError("Cannot delete employee {EmployeeId} with {SubordinatesCount} subordinates", id, subordinatesCount);
                    throw new InvalidOperationException("Cannot delete employee with subordinates.");
                }

            var employee = await _employeeRepository.GetByIdAsync(id);
            DeleteProfilePicture(employee!.ProfilePicture);

                var result = await _employeeRepository.DeleteAsync(id);
                stopwatch.Stop();
                _logger.LogInformation("Employee {EmployeeId} deleted successfully in {ElapsedMs}ms", id, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to delete employee {EmployeeId} after {ElapsedMs}ms", id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(EmployeePasswordChangeDto passwordChangeDto)
        {
            var employee = await _employeeRepository.GetByIdAsync(passwordChangeDto.EmployeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (employee.PasswordHash != HashPassword(passwordChangeDto.CurrentPassword))
            {
                throw new UnauthorizedAccessException("Current password is incorrect.");
            }

            employee.PasswordHash = HashPassword(passwordChangeDto.NewPassword);
            await _employeeRepository.UpdateAsync(employee);
            return true;
        }

        public async Task<bool> ActivateEmployeeAsync(string id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            employee.Status = EmployeeStatus.Active;
            await _employeeRepository.UpdateAsync(employee);
            return true;
        }

        public async Task<bool> DeactivateEmployeeAsync(string id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            employee.Status = EmployeeStatus.Inactive;
            await _employeeRepository.UpdateAsync(employee);
            return true;
        }

        private async Task<string?> SaveProfilePictureAsync(IFormFile? file, string employeeId)
        {
            if (file == null) return null;

            ValidateProfilePicture(file);

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{employeeId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/profiles/{fileName}";
        }

        private void DeleteProfilePicture(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // Log error but don't throw
            }
        }

        private static void ValidateProfilePicture(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Invalid file type. Only JPG, JPEG, PNG files are allowed.");
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB
            {
                throw new ArgumentException("File size cannot exceed 5MB.");
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private async Task<EmployeeResponseDto> MapToResponseDtoAsync(Employee employee, int subordinatesCount)
        {
            return new EmployeeResponseDto
            {
                EmployeeId = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                FullName = $"{employee.FirstName} {employee.LastName}",
                ProfilePicture = employee.ProfilePicture,
                MobileNumber = employee.MobileNumber,
                AlternateNumber = employee.AlternateNumber,
                PersonalEmail = employee.PersonalEmail,
                OfficeEmail = employee.OfficeEmail,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.DepartmentName ?? "",
                RoleId = employee.RoleId,
                RoleName = employee.Role?.RoleName ?? "",
                EmploymentType = employee.EmploymentType,
                DateOfJoining = employee.DateOfJoining,
                ReportingManagerId = employee.ReportingManagerId,
                ReportingManagerName = employee.ReportingManager != null ? $"{employee.ReportingManager.FirstName} {employee.ReportingManager.LastName}" : null,
                Status = employee.Status,
                NetSalary = employee.SalaryStructure?.NetSalary,
                SubordinatesCount = subordinatesCount
            };
        }
    }
}