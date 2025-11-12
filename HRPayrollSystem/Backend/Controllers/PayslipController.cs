using HRPayrollSystem_Payslip.DTOs.PayslipDTO;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayslipController : ControllerBase
    {
        private readonly IPayslipService _payslipService;

        public PayslipController(IPayslipService payslipService)
        {
            _payslipService = payslipService;
        }

        [HttpGet]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<PayslipResponseDto>>> GetAllPayslips()
        {
            try
            {
                var payslips = await _payslipService.GetAllPayslipsAsync();
                return Ok(payslips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving payslips.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<PayslipResponseDto>> GetPayslipById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid payslip ID." });
                }

                var payslip = await _payslipService.GetPayslipByIdAsync(id);
                if (payslip == null)
                {
                    return NotFound(new { message = "Payslip not found." });
                }

                return Ok(payslip);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the payslip.", error = ex.Message });
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<PayslipResponseDto>>> GetPayslipsByEmployeeId(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var payslips = await _payslipService.GetPayslipsByEmployeeIdAsync(employeeId);
                return Ok(payslips);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving payslips.", error = ex.Message });
            }
        }

        [HttpGet("payroll/{payrollId}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<PayslipResponseDto>> GetPayslipByPayrollId(int payrollId)
        {
            try
            {
                if (payrollId <= 0)
                {
                    return BadRequest(new { message = "Invalid payroll ID." });
                }

                var payslip = await _payslipService.GetPayslipByPayrollIdAsync(payrollId);
                if (payslip == null)
                {
                    return NotFound(new { message = "Payslip not found for this payroll." });
                }

                return Ok(payslip);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the payslip.", error = ex.Message });
            }
        }



        [HttpPut("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<PayslipResponseDto>> UpdatePayslip(int id, [FromForm] PayslipUpdateDto payslipUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid payslip ID." });
                }

                if (id != payslipUpdateDto.PayslipId)
                {
                    return BadRequest(new { message = "Payslip ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedPayslip = await _payslipService.UpdatePayslipAsync(payslipUpdateDto);
                return Ok(updatedPayslip);
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
                return StatusCode(500, new { message = "An error occurred while updating the payslip.", error = ex.Message });
            }
        }



        [HttpGet("{id}/download")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult> DownloadPayslip(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid payslip ID." });
                }

                var payslip = await _payslipService.GetPayslipByIdAsync(id);
                if (payslip == null)
                {
                    return NotFound(new { message = "Payslip not found." });
                }

                var fileBytes = await _payslipService.DownloadPayslipAsync(id);
                return File(fileBytes, "application/pdf", payslip.FileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while downloading the payslip.", error = ex.Message });
            }
        }
    }
}