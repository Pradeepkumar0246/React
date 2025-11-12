using HRPayrollSystem_Payslip.DTOs.PayrollDTO;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;
        private readonly ILogger<PayrollController> _logger;

        public PayrollController(IPayrollService payrollService, ILogger<PayrollController> logger)
        {
            _payrollService = payrollService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<PayrollResponseDto>>> GetAllPayrolls()
        {
            try
            {
                var payrolls = await _payrollService.GetAllPayrollsAsync();
                return Ok(payrolls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving payrolls.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<PayrollResponseDto>> GetPayrollById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid payroll ID." });
                }

                var payroll = await _payrollService.GetPayrollByIdAsync(id);
                if (payroll == null)
                {
                    _logger.LogWarning("Payroll {PayrollId} not found", id);
                    return NotFound(new { message = "Payroll not found." });
                }

                _logger.LogInformation("Retrieved payroll {PayrollId} successfully", id);
                return Ok(payroll);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the payroll.", error = ex.Message });
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<PayrollResponseDto>>> GetPayrollsByEmployeeId(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var payrolls = await _payrollService.GetPayrollsByEmployeeIdAsync(employeeId);
                return Ok(payrolls);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving payrolls.", error = ex.Message });
            }
        }

        [HttpGet("month")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<PayrollResponseDto>>> GetPayrollsByMonth([FromQuery] DateTime month)
        {
            try
            {
                var payrolls = await _payrollService.GetPayrollsByMonthAsync(month);
                return Ok(payrolls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving payrolls.", error = ex.Message });
            }
        }



        [HttpPut("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<PayrollResponseDto>> UpdatePayroll(int id, [FromBody] PayrollUpdateDto payrollUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid payroll ID." });
                }

                if (id != payrollUpdateDto.PayrollId)
                {
                    return BadRequest(new { message = "Payroll ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedPayroll = await _payrollService.UpdatePayrollAsync(payrollUpdateDto);
                return Ok(updatedPayroll);
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
                return StatusCode(500, new { message = "An error occurred while updating the payroll.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult> DeletePayroll(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid payroll ID." });
                }

                var result = await _payrollService.DeletePayrollAsync(id);
                if (result)
                {
                    return NoContent();
                }

                return NotFound(new { message = "Payroll not found." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the payroll.", error = ex.Message });
            }
        }

        [HttpPost("generate")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<PayrollResponseDto>> GeneratePayroll([FromBody] PayrollGenerateDto payrollGenerateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var generatedPayroll = await _payrollService.GeneratePayrollAsync(payrollGenerateDto);
                _logger.LogInformation("Payroll {PayrollId} generated successfully for employee {EmployeeId}", generatedPayroll.PayrollId, payrollGenerateDto.EmployeeId);
                return CreatedAtAction(nameof(GetPayrollById), new { id = generatedPayroll.PayrollId }, generatedPayroll);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the payroll.", error = ex.Message });
            }
        }
    }
}