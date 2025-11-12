using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Models
{
    public class Payroll
    {
        [Key]
        [Required]
        public int PayrollId { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public string EmployeeId { get; set; }

        [Required]
        public DateTime PayrollMonth { get; set; }   // Represents the month and year of the payroll

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal GrossSalary { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal TotalDeductions { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal NetPay { get; set; }

        [Range(0, double.MaxValue)]
        [Precision(18, 2)]
        public decimal Bonus { get; set; } = 0;

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; }

        // Navigation Properties
        public Employee? Employee { get; set; }
        public ICollection<Payslip>? Payslips { get; set; }
    }

}
