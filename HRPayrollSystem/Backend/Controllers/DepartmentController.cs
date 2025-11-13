using HRPayrollSystem_Payslip.DTOs.DepartmentDTO;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetAllDepartments()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving departments.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<DepartmentResponseDto>> GetDepartmentById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid department ID." });
                }

                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound(new { message = "Department not found." });
                }

                return Ok(department);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the department.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<DepartmentResponseDto>> CreateDepartment([FromBody] DepartmentCreateDto departmentCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdDepartment = await _departmentService.CreateDepartmentAsync(departmentCreateDto);
                return CreatedAtAction(nameof(GetDepartmentById), new { id = createdDepartment.DepartmentId }, new { message = "Department created successfully", result = createdDepartment });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the department.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<DepartmentResponseDto>> UpdateDepartment(int id, [FromBody] DepartmentUpdateDto departmentUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid department ID." });
                }

                if (id != departmentUpdateDto.DepartmentId)
                {
                    return BadRequest(new { message = "Department ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedDepartment = await _departmentService.UpdateDepartmentAsync(departmentUpdateDto);
                return Ok(new { message = "Department updated successfully", result = updatedDepartment });
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
                return StatusCode(500, new { message = "An error occurred while updating the department.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult> DeleteDepartment(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid department ID." });
                }

                var result = await _departmentService.DeleteDepartmentAsync(id);
                if (result)
                {
                    return Ok(new { message = "Department deleted successfully" });
                }

                return NotFound(new { message = "Department not found." });
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
                return StatusCode(500, new { message = "An error occurred while deleting the department.", error = ex.Message });
            }
        }
    }
}