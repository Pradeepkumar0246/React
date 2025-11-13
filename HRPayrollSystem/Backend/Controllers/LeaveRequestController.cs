using HRPayrollSystem_Payslip.DTOs.LeaveRequestDTO;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        [HttpGet]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<LeaveRequestResponseDto>>> GetAllLeaveRequests()
        {
            try
            {
                var leaveRequests = await _leaveRequestService.GetAllLeaveRequestsAsync();
                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving leave requests.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<LeaveRequestResponseDto>> GetLeaveRequestById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid leave request ID." });
                }

                var leaveRequest = await _leaveRequestService.GetLeaveRequestByIdAsync(id);
                if (leaveRequest == null)
                {
                    return NotFound(new { message = "Leave request not found." });
                }

                return Ok(leaveRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the leave request.", error = ex.Message });
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<LeaveRequestResponseDto>>> GetLeaveRequestsByEmployeeId(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var leaveRequests = await _leaveRequestService.GetLeaveRequestsByEmployeeIdAsync(employeeId);
                return Ok(leaveRequests);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving leave requests.", error = ex.Message });
            }
        }

        [HttpGet("status/{status}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<LeaveRequestResponseDto>>> GetLeaveRequestsByStatus(string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    return BadRequest(new { message = "Status is required." });
                }

                var leaveRequests = await _leaveRequestService.GetLeaveRequestsByStatusAsync(status);
                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving leave requests.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<LeaveRequestResponseDto>> CreateLeaveRequest([FromBody] LeaveRequestCreateDto leaveRequestCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdLeaveRequest = await _leaveRequestService.CreateLeaveRequestAsync(leaveRequestCreateDto);
                return CreatedAtAction(nameof(GetLeaveRequestById), new { id = createdLeaveRequest.LeaveRequestId }, new { message = "Leave request submitted successfully", result = createdLeaveRequest });
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
                return StatusCode(500, new { message = "An error occurred while creating the leave request.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<LeaveRequestResponseDto>> UpdateLeaveRequest(int id, [FromBody] LeaveRequestUpdateDto leaveRequestUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid leave request ID." });
                }

                if (id != leaveRequestUpdateDto.LeaveRequestId)
                {
                    return BadRequest(new { message = "Leave request ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedLeaveRequest = await _leaveRequestService.UpdateLeaveRequestAsync(leaveRequestUpdateDto);
                return Ok(new { message = "Leave request updated successfully", result = updatedLeaveRequest });
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
                return StatusCode(500, new { message = "An error occurred while updating the leave request.", error = ex.Message });
            }
        }



        [HttpPut("{id}/status")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<LeaveRequestResponseDto>> UpdateLeaveRequestStatus(int id, [FromBody] LeaveRequestStatusUpdateDto statusUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid leave request ID." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedLeaveRequest = await _leaveRequestService.UpdateLeaveRequestStatusAsync(id, statusUpdateDto.Status, statusUpdateDto.ApprovedBy);
                return Ok(new { message = "Leave request updated successfully", result = updatedLeaveRequest });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the leave request status.", error = ex.Message });
            }
        }
    }
}