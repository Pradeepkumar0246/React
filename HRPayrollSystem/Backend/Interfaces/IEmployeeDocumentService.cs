using HRPayrollSystem_Payslip.DTOs.EmployeeDocumentDTO;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IEmployeeDocumentService
    {
        Task<IEnumerable<EmployeeDocumentResponseDto>> GetAllDocumentsAsync();
        Task<EmployeeDocumentResponseDto?> GetDocumentByIdAsync(int id);
        Task<IEnumerable<EmployeeDocumentResponseDto>> GetDocumentsByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<EmployeeDocumentResponseDto>> GetDocumentsByCategoryAsync(string category);
        Task<EmployeeDocumentResponseDto> CreateDocumentAsync(EmployeeDocumentCreateDto employeeDocumentCreateDto);
        Task<EmployeeDocumentResponseDto> UpdateDocumentAsync(EmployeeDocumentUpdateDto employeeDocumentUpdateDto);
        Task<bool> DeleteDocumentAsync(int id);
        Task<byte[]> DownloadDocumentAsync(int id);
    }
}