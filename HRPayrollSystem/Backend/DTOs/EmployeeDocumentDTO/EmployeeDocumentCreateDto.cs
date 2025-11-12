using HRPayrollSystem_Payslip.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRPayrollSystem_Payslip.DTOs.EmployeeDocumentDTO
{
    public class EmployeeDocumentCreateDto
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public DocumentCategory Category { get; set; }

        [Required(ErrorMessage = "Document file is required.")]
        public IFormFile DocumentFile { get; set; }
    }
}