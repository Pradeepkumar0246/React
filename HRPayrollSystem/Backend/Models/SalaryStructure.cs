using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.Models
{
    public class SalaryStructure
    {
        [Key]
        [Required]
        public int SalaryStructureId { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        [Required, Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal BasicSalary { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal HRA { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal Allowances { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal Deductions { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal PF { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal Tax { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal NetSalary { get; set; }

        // Navigation Property
        public Employee? Employee { get; set; }
    }

}
