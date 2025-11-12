using HRPayrollSystem_Payslip.DTOs.AttendanceDTO;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpGet]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<AttendanceResponseDto>>> GetAllAttendance()
        {
            try
            {
                var attendances = await _attendanceService.GetAllAttendanceAsync();
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving attendance records.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<AttendanceResponseDto>> GetAttendanceById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid attendance ID." });
                }

                var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
                if (attendance == null)
                {
                    return NotFound(new { message = "Attendance record not found." });
                }

                return Ok(attendance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the attendance record.", error = ex.Message });
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<AttendanceResponseDto>>> GetAttendanceByEmployeeId(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var attendances = await _attendanceService.GetAttendanceByEmployeeIdAsync(employeeId);
                return Ok(attendances);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving attendance records.", error = ex.Message });
            }
        }

        [HttpGet("daterange")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<AttendanceResponseDto>>> GetAttendanceByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                var attendances = await _attendanceService.GetAttendanceByDateRangeAsync(fromDate, toDate);
                return Ok(attendances);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving attendance records.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<AttendanceResponseDto>> CreateAttendance([FromBody] AttendanceCreateDto attendanceCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdAttendance = await _attendanceService.CreateAttendanceAsync(attendanceCreateDto);
                return CreatedAtAction(nameof(GetAttendanceById), new { id = createdAttendance.AttendanceId }, createdAttendance);
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
                return StatusCode(500, new { message = "An error occurred while creating the attendance record.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<AttendanceResponseDto>> UpdateAttendance(int id, [FromBody] AttendanceUpdateDto attendanceUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid attendance ID." });
                }

                if (id != attendanceUpdateDto.AttendanceId)
                {
                    return BadRequest(new { message = "Attendance ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedAttendance = await _attendanceService.UpdateAttendanceAsync(attendanceUpdateDto);
                return Ok(updatedAttendance);
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
                return StatusCode(500, new { message = "An error occurred while updating the attendance record.", error = ex.Message });
            }
        }



        [HttpPost("checkin/{employeeId}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<AttendanceResponseDto>> CheckIn(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var attendance = await _attendanceService.CheckInAsync(employeeId);
                return Ok(attendance);
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
                return StatusCode(500, new { message = "An error occurred during check-in.", error = ex.Message });
            }
        }

        [HttpPut("checkout/{employeeId}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<AttendanceResponseDto>> CheckOut(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var attendance = await _attendanceService.CheckOutAsync(employeeId);
                return Ok(attendance);
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
                return StatusCode(500, new { message = "An error occurred during check-out.", error = ex.Message });
            }
        }
    }
}