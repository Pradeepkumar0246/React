using HRPayrollSystem_Payslip.DTOs.AuditLogDTO;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(IAuditLogService auditLogService, ILogger<AuditLogController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> GetAllAuditLogs()
        {
            try
            {
                var auditLogs = await _auditLogService.GetAllAuditLogsAsync();
                _logger.LogInformation("Retrieved {Count} audit logs successfully", auditLogs.Count());
                return Ok(auditLogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<AuditLogResponseDto>> GetAuditLogById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid audit log ID." });
                }

                var auditLog = await _auditLogService.GetAuditLogByIdAsync(id);
                if (auditLog == null)
                {
                    _logger.LogWarning("Audit log {AuditLogId} not found", id);
                    return NotFound(new { message = "Audit log not found." });
                }

                _logger.LogInformation("Retrieved audit log {AuditLogId} successfully", id);
                return Ok(auditLog);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the audit log.", error = ex.Message });
            }
        }

        [HttpGet("action/{actionType}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> GetAuditLogsByActionType(string actionType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(actionType))
                {
                    return BadRequest(new { message = "Action type is required." });
                }

                var auditLogs = await _auditLogService.GetAuditLogsByActionTypeAsync(actionType);
                return Ok(auditLogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs.", error = ex.Message });
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> GetAuditLogsByEmployeeId(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var auditLogs = await _auditLogService.GetAuditLogsByEmployeeIdAsync(employeeId);
                return Ok(auditLogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs.", error = ex.Message });
            }
        }

        [HttpGet("daterange")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> GetAuditLogsByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                var auditLogs = await _auditLogService.GetAuditLogsByDateRangeAsync(fromDate, toDate);
                return Ok(auditLogs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs.", error = ex.Message });
            }
        }

    }
}