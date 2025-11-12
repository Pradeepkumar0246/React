using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRPayrollSystem_Payslip.Models
{
    public class Payslip
    {
        [Key]
        [Required]
        public int PayslipId { get; set; }

        [Required]
        public string EmployeeId { get; set; } // FK → Employee

        [Required]
        public int PayrollId { get; set; } // FK → Payroll

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } // path or URL of generated payslip file

        [Required]
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("PayrollId")]
        public Payroll Payroll { get; set; }
    }

}
