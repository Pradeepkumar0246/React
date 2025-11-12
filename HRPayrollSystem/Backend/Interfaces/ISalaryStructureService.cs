using HRPayrollSystem_Payslip.DTOs.SalaryStructureDTO;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface ISalaryStructureService
    {
        Task<IEnumerable<SalaryStructureResponseDto>> GetAllSalaryStructuresAsync();
        Task<SalaryStructureResponseDto?> GetSalaryStructureByIdAsync(int id);
        Task<SalaryStructureResponseDto?> GetSalaryStructureByEmployeeIdAsync(string employeeId);
        Task<SalaryStructureResponseDto> CreateSalaryStructureAsync(SalaryStructureCreateDto salaryStructureCreateDto);
        Task<SalaryStructureResponseDto> UpdateSalaryStructureAsync(SalaryStructureUpdateDto salaryStructureUpdateDto);

    }
}