using HRPayrollSystem_Payslip.DTOs;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
    }
}