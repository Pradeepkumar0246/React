using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.DTOs.SalaryStructureDTO
{
    public class SalaryStructureUpdateDto
    {
        [Required(ErrorMessage = "Salary structure ID is required.")]
        public int SalaryStructureId { get; set; }

        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Basic salary is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Basic salary must be non-negative.")]
        public decimal BasicSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "HRA must be non-negative.")]
        public decimal HRA { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Allowances must be non-negative.")]
        public decimal Allowances { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Deductions must be non-negative.")]
        public decimal Deductions { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "PF must be non-negative.")]
        public decimal PF { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tax must be non-negative.")]
        public decimal Tax { get; set; }
    }
}