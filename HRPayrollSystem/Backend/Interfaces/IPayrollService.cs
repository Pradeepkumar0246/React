using HRPayrollSystem_Payslip.DTOs.PayrollDTO;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IPayrollService
    {
        Task<IEnumerable<PayrollResponseDto>> GetAllPayrollsAsync();
        Task<PayrollResponseDto?> GetPayrollByIdAsync(int id);
        Task<IEnumerable<PayrollResponseDto>> GetPayrollsByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<PayrollResponseDto>> GetPayrollsByMonthAsync(DateTime month);

        Task<PayrollResponseDto> UpdatePayrollAsync(PayrollUpdateDto payrollUpdateDto);
        Task<bool> DeletePayrollAsync(int id);
        Task<PayrollResponseDto> GeneratePayrollAsync(DTOs.PayrollDTO.PayrollGenerateDto payrollGenerateDto);
    }
}