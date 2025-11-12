using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveBalanceController : ControllerBase
    {
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveBalanceController(ILeaveBalanceService leaveBalanceService)
        {
            _leaveBalanceService = leaveBalanceService;
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<IActionResult> GetEmployeeLeaveBalances(string employeeId, [FromQuery] int? year = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var balances = await _leaveBalanceService.GetEmployeeLeaveBalancesAsync(employeeId, year);
                return Ok(balances);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("year-end-process")]
        [Authorize(Policy = "FullAccess")]
        public async Task<IActionResult> ProcessYearEnd()
        {
            try
            {
                await _leaveBalanceService.ProcessYearEndAsync();
                return Ok(new { message = "Year-end processing completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}