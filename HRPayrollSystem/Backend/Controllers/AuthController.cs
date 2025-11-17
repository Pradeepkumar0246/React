using HRPayrollSystem_Payslip.DTOs;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRPayrollSystem_Payslip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new { message ="login successful",result});
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var employeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(employeeId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var userDetails = await _authService.GetCurrentUserAsync(employeeId);
                if (userDetails == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user details.", error = ex.Message });
            }
        }
    }
}