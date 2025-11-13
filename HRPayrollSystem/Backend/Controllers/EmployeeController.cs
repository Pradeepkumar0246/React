using HRPayrollSystem_Payslip.DTOs.EmployeeDTO;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAllEmployees()
        {
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync();
                _logger.LogInformation("Retrieved {Count} employees successfully", employees.Count());
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving employees.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found", id);
                    return NotFound(new { message = "Employee not found." });
                }

                _logger.LogInformation("Retrieved employee {EmployeeId} successfully", id);
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the employee.", error = ex.Message });
            }
        }

        [HttpGet("department/{departmentId}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployeesByDepartmentId(int departmentId)
        {
            try
            {
                if (departmentId <= 0)
                {
                    return BadRequest(new { message = "Invalid department ID." });
                }

                var employees = await _employeeService.GetEmployeesByDepartmentIdAsync(departmentId);
                return Ok(employees);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving employees.", error = ex.Message });
            }
        }

        [HttpGet("role/{roleId}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployeesByRoleId(int roleId)
        {
            try
            {
                if (roleId <= 0)
                {
                    return BadRequest(new { message = "Invalid role ID." });
                }

                var employees = await _employeeService.GetEmployeesByRoleIdAsync(roleId);
                return Ok(employees);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving employees.", error = ex.Message });
            }
        }

        [HttpGet("subordinates/{managerId}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetSubordinates(string managerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(managerId))
                {
                    return BadRequest(new { message = "Manager ID is required." });
                }

                var subordinates = await _employeeService.GetSubordinatesAsync(managerId);
                return Ok(subordinates);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving subordinates.", error = ex.Message });
            }
        }

        [HttpGet("search")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> SearchEmployees([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new { message = "Search term is required." });
                }

                var employees = await _employeeService.SearchEmployeesAsync(searchTerm);
                return Ok(employees);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching employees.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<EmployeeResponseDto>> CreateEmployee([FromForm] EmployeeCreateDto employeeCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdEmployee = await _employeeService.CreateEmployeeAsync(employeeCreateDto);
                _logger.LogInformation("Employee {EmployeeId} created successfully", createdEmployee.EmployeeId);
                return CreatedAtAction(nameof(GetEmployeeById), new { id = createdEmployee.EmployeeId }, new { message = "Employee created successfully", result = createdEmployee });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the employee.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<EmployeeResponseDto>> UpdateEmployee(string id, [FromForm] EmployeeUpdateDto employeeUpdateDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                if (id != employeeUpdateDto.EmployeeId)
                {
                    return BadRequest(new { message = "Employee ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedEmployee = await _employeeService.UpdateEmployeeAsync(employeeUpdateDto);
                _logger.LogInformation("Employee {EmployeeId} updated successfully", id);
                return Ok(new { message = "Employee updated successfully", result = updatedEmployee });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the employee.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult> DeleteEmployee(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var result = await _employeeService.DeleteEmployeeAsync(id);
                if (result)
                {
                    _logger.LogInformation("Employee {EmployeeId} deleted successfully", id);
                    return Ok(new { message = "Employee deleted successfully" });
                }

                _logger.LogWarning("Employee {EmployeeId} not found for deletion", id);
                return NotFound(new { message = "Employee not found." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the employee.", error = ex.Message });
            }
        }

        [HttpPost("{id}/change-password")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult> ChangePassword(string id, [FromBody] EmployeePasswordChangeDto passwordChangeDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                if (id != passwordChangeDto.EmployeeId)
                {
                    return BadRequest(new { message = "Employee ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _employeeService.ChangePasswordAsync(passwordChangeDto);
                if (result)
                {
                    return Ok(new { message = "Password changed successfully." });
                }

                return BadRequest(new { message = "Failed to change password." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while changing the password.", error = ex.Message });
            }
        }

        [HttpPost("{id}/activate")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult> ActivateEmployee(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var result = await _employeeService.ActivateEmployeeAsync(id);
                if (result)
                {
                    return Ok(new { message = "Employee activated successfully." });
                }

                return BadRequest(new { message = "Failed to activate employee." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while activating the employee.", error = ex.Message });
            }
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult> DeactivateEmployee(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var result = await _employeeService.DeactivateEmployeeAsync(id);
                if (result)
                {
                    return Ok(new { message = "Employee deactivated successfully." });
                }

                return BadRequest(new { message = "Failed to deactivate employee." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deactivating the employee.", error = ex.Message });
            }
        }
    }
}