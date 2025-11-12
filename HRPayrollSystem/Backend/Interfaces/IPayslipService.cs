using HRPayrollSystem_Payslip.DTOs.PayslipDTO;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IPayslipService
    {
        Task<IEnumerable<PayslipResponseDto>> GetAllPayslipsAsync();
        Task<PayslipResponseDto?> GetPayslipByIdAsync(int id);
        Task<IEnumerable<PayslipResponseDto>> GetPayslipsByEmployeeIdAsync(string employeeId);
        Task<PayslipResponseDto?> GetPayslipByPayrollIdAsync(int payrollId);
        Task<PayslipResponseDto> UpdatePayslipAsync(PayslipUpdateDto payslipUpdateDto);
        Task<byte[]> DownloadPayslipAsync(int id);
        Task<string> GeneratePayslipAsync(string employeeId, int payrollId);
    }
}