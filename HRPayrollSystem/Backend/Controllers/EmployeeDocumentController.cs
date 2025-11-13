using HRPayrollSystem_Payslip.DTOs.EmployeeDocumentDTO;
using HRPayrollSystem_Payslip.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayrollSystem_Payslip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeDocumentController : ControllerBase
    {
        private readonly IEmployeeDocumentService _employeeDocumentService;
        private readonly ILogger<EmployeeDocumentController> _logger;

        public EmployeeDocumentController(IEmployeeDocumentService employeeDocumentService, ILogger<EmployeeDocumentController> logger)
        {
            _employeeDocumentService = employeeDocumentService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<IEnumerable<EmployeeDocumentResponseDto>>> GetAllDocuments()
        {
            try
            {
                var documents = await _employeeDocumentService.GetAllDocumentsAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving documents.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<EmployeeDocumentResponseDto>> GetDocumentById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid document ID." });
                }

                var document = await _employeeDocumentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound(new { message = "Document not found." });
                }

                return Ok(document);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the document.", error = ex.Message });
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<EmployeeDocumentResponseDto>>> GetDocumentsByEmployeeId(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    return BadRequest(new { message = "Employee ID is required." });
                }

                var documents = await _employeeDocumentService.GetDocumentsByEmployeeIdAsync(employeeId);
                return Ok(documents);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving documents.", error = ex.Message });
            }
        }

        [HttpGet("category/{category}")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<IEnumerable<EmployeeDocumentResponseDto>>> GetDocumentsByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return BadRequest(new { message = "Category is required." });
                }

                var documents = await _employeeDocumentService.GetDocumentsByCategoryAsync(category);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving documents.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult<EmployeeDocumentResponseDto>> CreateDocument([FromForm] EmployeeDocumentCreateDto employeeDocumentCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdDocument = await _employeeDocumentService.CreateDocumentAsync(employeeDocumentCreateDto);
                _logger.LogInformation("Document {DocumentId} created successfully for employee {EmployeeId}", createdDocument.DocumentId, employeeDocumentCreateDto.EmployeeId);
                return CreatedAtAction(nameof(GetDocumentById), new { id = createdDocument.DocumentId }, new { message = "Document uploaded successfully", result = createdDocument });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the document.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult<EmployeeDocumentResponseDto>> UpdateDocument(int id, [FromForm] EmployeeDocumentUpdateDto employeeDocumentUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid document ID." });
                }

                if (id != employeeDocumentUpdateDto.DocumentId)
                {
                    return BadRequest(new { message = "Document ID mismatch." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedDocument = await _employeeDocumentService.UpdateDocumentAsync(employeeDocumentUpdateDto);
                return Ok(new { message = "Document updated successfully", result = updatedDocument });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the document.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "FullAccess")]
        public async Task<ActionResult> DeleteDocument(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid document ID." });
                }

                var result = await _employeeDocumentService.DeleteDocumentAsync(id);
                if (result)
                {
                    _logger.LogInformation("Document {DocumentId} deleted successfully", id);
                    return Ok(new { message = "Document deleted successfully" });
                }

                _logger.LogWarning("Document {DocumentId} not found for deletion", id);
                return NotFound(new { message = "Document not found." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the document.", error = ex.Message });
            }
        }

        [HttpGet("{id}/download")]
        [Authorize(Policy = "AllAccess")]
        public async Task<ActionResult> DownloadDocument(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid document ID." });
                }

                var document = await _employeeDocumentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound(new { message = "Document not found." });
                }

                var fileBytes = await _employeeDocumentService.DownloadDocumentAsync(id);
                var contentType = GetContentType(document.FileName);
                
                return File(fileBytes, contentType, document.FileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while downloading the document.", error = ex.Message });
            }
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}