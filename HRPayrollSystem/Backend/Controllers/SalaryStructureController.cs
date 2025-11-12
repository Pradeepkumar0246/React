using HRPayrollSystem_Payslip.DTOs.SalaryStructureDTO;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalaryStructureController : ControllerBase
    {
        private readonly ISalaryStructureService _salaryStructureService;

        public SalaryStructureController(ISalaryStructureService salaryStructureService)
        {
            _salaryStructureService = salaryStructureService;
        }

        [HttpGet]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<SalaryStructureResponseDto>>> GetAllSalaryStructures()
        {
            try
            {
                var salaryStructures = await _salaryStructureService.GetAllSalaryStructuresAsync();
                return Ok(salaryStructures);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving salary structures.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<SalaryStructureResponseDto>> GetSalaryStructureById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid salary structure ID." });
                }

                var salaryStructure = await _salaryStructureService.GetSalaryStructureByIdAsync(id);
                if (salaryStructure == null)
                {
                    return NotFound(new { message = "Salary structure not found." });
                }

                return Ok(salaryStructure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the salary structure.", error = ex.Message });
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Policy = "FullAccess,LimitedAccess")]
        public async Task<ActionResult<SalaryStructureResponseDto>> GetSalaryStructureByEmployeeId(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var salaryStructure = await _salaryStructureService.GetSalaryStructureByEmployeeIdAsync(employeeId);
                if (salaryStructure == null)
                {
                    return NotFound(new { message = "Salary structure not found for this employee." });
                }

                return Ok(salaryStructure);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the salary structure.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<SalaryStructureResponseDto>> CreateSalaryStructure([FromBody] SalaryStructureCreateDto salaryStructureCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdSalaryStructure = await _salaryStructureService.CreateSalaryStructureAsync(salaryStructureCreateDto);
                return CreatedAtAction(nameof(GetSalaryStructureById), new { id = createdSalaryStructure.SalaryStructureId }, createdSalaryStructure);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the salary structure.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<SalaryStructureResponseDto>> UpdateSalaryStructure(int id, [FromBody] SalaryStructureUpdateDto salaryStructureUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid salary structure ID." });
                }

                if (id != salaryStructureUpdateDto.SalaryStructureId)
                {
                    return BadRequest(new { message = "Salary structure ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedSalaryStructure = await _salaryStructureService.UpdateSalaryStructureAsync(salaryStructureUpdateDto);
                return Ok(updatedSalaryStructure);
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
                return StatusCode(500, new { message = "An error occurred while updating the salary structure.", error = ex.Message });
            }
        }


    }
}