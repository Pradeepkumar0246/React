using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.EmployeeDocumentDTO
{
    public class EmployeeDocumentResponseDto
    {
        public int DocumentId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DocumentCategory Category { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedDate { get; set; }
        public long FileSize { get; set; }
    }
}