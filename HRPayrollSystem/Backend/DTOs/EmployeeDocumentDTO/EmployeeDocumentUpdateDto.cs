using System.ComponentModel.DataAnnotations;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.EmployeeDocumentDTO
{
    public class EmployeeDocumentUpdateDto
    {
        [Required(ErrorMessage = "Document ID is required.")]
        public int DocumentId { get; set; }

        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public DocumentCategory Category { get; set; }

        public IFormFile? DocumentFile { get; set; }
    }
}