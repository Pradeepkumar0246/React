using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Models
{
    public class EmployeeDocument
    {
        [Key]
        [Required]
        public int DocumentId { get; set; }

        [Required]
        public string EmployeeId { get; set; } // FK → Employee

        [Required]
        public DocumentCategory Category { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } // path or URL of the uploaded file

        [Required]
        public DateTime UploadedDate { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
    }

}
