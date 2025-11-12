using HRPayrollSystem_Payslip.DTOs.SalaryStructureDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Services
{
    public class SalaryStructureService : ISalaryStructureService
    {
        private readonly ISalaryStructureRepository _salaryStructureRepository;

        public SalaryStructureService(ISalaryStructureRepository salaryStructureRepository)
        {
            _salaryStructureRepository = salaryStructureRepository;
        }

        public async Task<IEnumerable<SalaryStructureResponseDto>> GetAllSalaryStructuresAsync()
        {
            var salaryStructures = await _salaryStructureRepository.GetAllAsync();
            return salaryStructures.Select(MapToResponseDto);
        }

        public async Task<SalaryStructureResponseDto?> GetSalaryStructureByIdAsync(int id)
        {
            var salaryStructure = await _salaryStructureRepository.GetByIdAsync(id);
            return salaryStructure == null ? null : MapToResponseDto(salaryStructure);
        }

        public async Task<SalaryStructureResponseDto?> GetSalaryStructureByEmployeeIdAsync(string employeeId)
        {
            if (!await _salaryStructureRepository.EmployeeExistsAsync(employeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var salaryStructure = await _salaryStructureRepository.GetByEmployeeIdAsync(employeeId);
            return salaryStructure == null ? null : MapToResponseDto(salaryStructure);
        }

        public async Task<SalaryStructureResponseDto> CreateSalaryStructureAsync(SalaryStructureCreateDto salaryStructureCreateDto)
        {
            if (!await _salaryStructureRepository.EmployeeExistsAsync(salaryStructureCreateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (await _salaryStructureRepository.EmployeeHasSalaryStructureAsync(salaryStructureCreateDto.EmployeeId))
            {
                throw new InvalidOperationException("Employee already has a salary structure.");
            }

            var netSalary = CalculateNetSalary(salaryStructureCreateDto.BasicSalary, salaryStructureCreateDto.HRA, 
                salaryStructureCreateDto.Allowances, salaryStructureCreateDto.Deductions, 
                salaryStructureCreateDto.PF, salaryStructureCreateDto.Tax);

            var salaryStructure = new SalaryStructure
            {
                EmployeeId = salaryStructureCreateDto.EmployeeId,
                BasicSalary = salaryStructureCreateDto.BasicSalary,
                HRA = salaryStructureCreateDto.HRA,
                Allowances = salaryStructureCreateDto.Allowances,
                Deductions = salaryStructureCreateDto.Deductions,
                PF = salaryStructureCreateDto.PF,
                Tax = salaryStructureCreateDto.Tax,
                NetSalary = netSalary
            };

            var createdSalaryStructure = await _salaryStructureRepository.CreateAsync(salaryStructure);
            var result = await _salaryStructureRepository.GetByIdAsync(createdSalaryStructure.SalaryStructureId);
            return MapToResponseDto(result!);
        }

        public async Task<SalaryStructureResponseDto> UpdateSalaryStructureAsync(SalaryStructureUpdateDto salaryStructureUpdateDto)
        {
            if (!await _salaryStructureRepository.ExistsAsync(salaryStructureUpdateDto.SalaryStructureId))
            {
                throw new KeyNotFoundException("Salary structure not found.");
            }

            if (!await _salaryStructureRepository.EmployeeExistsAsync(salaryStructureUpdateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (await _salaryStructureRepository.EmployeeHasSalaryStructureAsync(salaryStructureUpdateDto.EmployeeId, salaryStructureUpdateDto.SalaryStructureId))
            {
                throw new InvalidOperationException("Employee already has a different salary structure.");
            }

            var netSalary = CalculateNetSalary(salaryStructureUpdateDto.BasicSalary, salaryStructureUpdateDto.HRA, 
                salaryStructureUpdateDto.Allowances, salaryStructureUpdateDto.Deductions, 
                salaryStructureUpdateDto.PF, salaryStructureUpdateDto.Tax);

            var salaryStructure = new SalaryStructure
            {
                SalaryStructureId = salaryStructureUpdateDto.SalaryStructureId,
                EmployeeId = salaryStructureUpdateDto.EmployeeId,
                BasicSalary = salaryStructureUpdateDto.BasicSalary,
                HRA = salaryStructureUpdateDto.HRA,
                Allowances = salaryStructureUpdateDto.Allowances,
                Deductions = salaryStructureUpdateDto.Deductions,
                PF = salaryStructureUpdateDto.PF,
                Tax = salaryStructureUpdateDto.Tax,
                NetSalary = netSalary
            };

            var updatedSalaryStructure = await _salaryStructureRepository.UpdateAsync(salaryStructure);
            var result = await _salaryStructureRepository.GetByIdAsync(updatedSalaryStructure.SalaryStructureId);
            return MapToResponseDto(result!);
        }



        private static decimal CalculateNetSalary(decimal basicSalary, decimal hra, decimal allowances, decimal deductions, decimal pf, decimal tax)
        {
            var grossSalary = basicSalary + hra + allowances;
            var totalDeductions = deductions + pf + tax;
            return grossSalary - totalDeductions;
        }

        private static SalaryStructureResponseDto MapToResponseDto(SalaryStructure salaryStructure)
        {
            var grossSalary = salaryStructure.BasicSalary + salaryStructure.HRA + salaryStructure.Allowances;
            
            return new SalaryStructureResponseDto
            {
                SalaryStructureId = salaryStructure.SalaryStructureId,
                EmployeeId = salaryStructure.EmployeeId,
                EmployeeName = $"{salaryStructure.Employee?.FirstName} {salaryStructure.Employee?.LastName}".Trim(),
                BasicSalary = salaryStructure.BasicSalary,
                HRA = salaryStructure.HRA,
                Allowances = salaryStructure.Allowances,
                Deductions = salaryStructure.Deductions,
                PF = salaryStructure.PF,
                Tax = salaryStructure.Tax,
                NetSalary = salaryStructure.NetSalary,
                GrossSalary = grossSalary
            };
        }
    }
}