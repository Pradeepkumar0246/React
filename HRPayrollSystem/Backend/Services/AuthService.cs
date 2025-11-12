using HRPayrollSystem_Payslip.DTOs;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HRPayrollSystem_Payslip.Services
{
    public class AuthService : IAuthService
    {
        private readonly HRPayrollDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(HRPayrollDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var employee = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.OfficeEmail == loginDto.OfficeEmail);

            if (employee == null || !VerifyPassword(loginDto.Password, employee.PasswordHash))
                return null;

            var token = GenerateJwtToken(employee);

            return new LoginResponseDto
            {
                Token = token,
                EmployeeId = employee.EmployeeId,
                Name = $"{employee.FirstName} {employee.LastName}",
                Role = employee.Role?.RoleName ?? "Unknown",
                Profilepicture=employee.ProfilePicture
            };
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hashedInput = Convert.ToBase64String(hashedBytes);
            return hashedInput == hashedPassword;
        }

        private string GenerateJwtToken(Models.Employee employee)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId),
                new Claim(ClaimTypes.Email, employee.OfficeEmail),
                new Claim(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}"),
                new Claim(ClaimTypes.Role, employee.Role?.RoleName ?? "Unknown")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
