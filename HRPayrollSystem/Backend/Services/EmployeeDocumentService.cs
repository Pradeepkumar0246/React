using HRPayrollSystem_Payslip.DTOs.EmployeeDocumentDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Services
{
    public class EmployeeDocumentService : IEmployeeDocumentService
    {
        private readonly IEmployeeDocumentRepository _employeeDocumentRepository;
        private readonly IWebHostEnvironment _environment;

        public EmployeeDocumentService(IEmployeeDocumentRepository employeeDocumentRepository, IWebHostEnvironment environment)
        {
            _employeeDocumentRepository = employeeDocumentRepository;
            _environment = environment;
        }

        public async Task<IEnumerable<EmployeeDocumentResponseDto>> GetAllDocumentsAsync()
        {
            var documents = await _employeeDocumentRepository.GetAllAsync();
            return documents.Select(MapToResponseDto);
        }

        public async Task<EmployeeDocumentResponseDto?> GetDocumentByIdAsync(int id)
        {
            var document = await _employeeDocumentRepository.GetByIdAsync(id);
            return document == null ? null : MapToResponseDto(document);
        }

        public async Task<IEnumerable<EmployeeDocumentResponseDto>> GetDocumentsByEmployeeIdAsync(string employeeId)
        {
            if (!await _employeeDocumentRepository.EmployeeExistsAsync(employeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var documents = await _employeeDocumentRepository.GetByEmployeeIdAsync(employeeId);
            return documents.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<EmployeeDocumentResponseDto>> GetDocumentsByCategoryAsync(string category)
        {
            var documents = await _employeeDocumentRepository.GetByCategoryAsync(category);
            return documents.Select(MapToResponseDto);
        }

        public async Task<EmployeeDocumentResponseDto> CreateDocumentAsync(EmployeeDocumentCreateDto employeeDocumentCreateDto)
        {
            if (!await _employeeDocumentRepository.EmployeeExistsAsync(employeeDocumentCreateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            ValidateFileType(employeeDocumentCreateDto.DocumentFile);

            var filePath = await SaveFileAsync(employeeDocumentCreateDto.DocumentFile, employeeDocumentCreateDto.EmployeeId, employeeDocumentCreateDto.Category);

            var document = new EmployeeDocument
            {
                EmployeeId = employeeDocumentCreateDto.EmployeeId,
                Category = employeeDocumentCreateDto.Category,
                FileName = employeeDocumentCreateDto.DocumentFile.FileName,
                FilePath = filePath,
                UploadedDate = DateTime.Now
            };

            var createdDocument = await _employeeDocumentRepository.CreateAsync(document);
            var result = await _employeeDocumentRepository.GetByIdAsync(createdDocument.DocumentId);
            return MapToResponseDto(result!);
        }

        public async Task<EmployeeDocumentResponseDto> UpdateDocumentAsync(EmployeeDocumentUpdateDto employeeDocumentUpdateDto)
        {
            if (!await _employeeDocumentRepository.ExistsAsync(employeeDocumentUpdateDto.DocumentId))
            {
                throw new KeyNotFoundException("Document not found.");
            }

            if (!await _employeeDocumentRepository.EmployeeExistsAsync(employeeDocumentUpdateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var existingDocument = await _employeeDocumentRepository.GetByIdAsync(employeeDocumentUpdateDto.DocumentId);
            var filePath = existingDocument!.FilePath;
            var fileName = existingDocument.FileName;

            if (employeeDocumentUpdateDto.DocumentFile != null)
            {
                ValidateFileType(employeeDocumentUpdateDto.DocumentFile);
                DeleteFile(filePath);
                filePath = await SaveFileAsync(employeeDocumentUpdateDto.DocumentFile, employeeDocumentUpdateDto.EmployeeId, employeeDocumentUpdateDto.Category);
                fileName = employeeDocumentUpdateDto.DocumentFile.FileName;
            }

            var document = new EmployeeDocument
            {
                DocumentId = employeeDocumentUpdateDto.DocumentId,
                EmployeeId = employeeDocumentUpdateDto.EmployeeId,
                Category = employeeDocumentUpdateDto.Category,
                FileName = fileName,
                FilePath = filePath,
                UploadedDate = existingDocument.UploadedDate
            };

            var updatedDocument = await _employeeDocumentRepository.UpdateAsync(document);
            var result = await _employeeDocumentRepository.GetByIdAsync(updatedDocument.DocumentId);
            return MapToResponseDto(result!);
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var document = await _employeeDocumentRepository.GetByIdAsync(id);
            if (document == null)
            {
                throw new KeyNotFoundException("Document not found.");
            }

            DeleteFile(document.FilePath);
            return await _employeeDocumentRepository.DeleteAsync(id);
        }

        public async Task<byte[]> DownloadDocumentAsync(int id)
        {
            var document = await _employeeDocumentRepository.GetByIdAsync(id);
            if (document == null)
            {
                throw new KeyNotFoundException("Document not found.");
            }

            var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Document file not found.");
            }

            return await File.ReadAllBytesAsync(fullPath);
        }

        private static void ValidateFileType(IFormFile file)
        {
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Invalid file type. Only PDF, DOC, DOCX, JPG, JPEG, PNG files are allowed.");
            }

            if (file.Length > 10 * 1024 * 1024) // 10MB
            {
                throw new ArgumentException("File size cannot exceed 10MB.");
            }
        }

        private async Task<string> SaveFileAsync(IFormFile file, string employeeId, DocumentCategory category)
        {
            var categoryString = category.ToString();
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents", employeeId, categoryString);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/documents/{employeeId}/{categoryString}/{fileName}";
        }

        private void DeleteFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // Log error but don't throw
            }
        }

        private EmployeeDocumentResponseDto MapToResponseDto(EmployeeDocument document)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            var fileSize = File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;

            return new EmployeeDocumentResponseDto
            {
                DocumentId = document.DocumentId,
                EmployeeId = document.EmployeeId,
                EmployeeName = $"{document.Employee?.FirstName} {document.Employee?.LastName}".Trim(),
                Category = document.Category,
                FileName = document.FileName,
                FilePath = document.FilePath,
                UploadedDate = document.UploadedDate,
                FileSize = fileSize
            };
        }
    }
}