using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.API.DTOs;
using TechMoveGLMS_Web.Data;
using TechMoveGLMS_Web.Models;
using TechMoveGLMS.API.Services;

namespace TechMoveGLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IContractRepository _contractRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContractsController(
            ApplicationDbContext context,
            IContractRepository contractRepository,
            IFileUploadService fileUploadService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _contractRepository = contractRepository;
            _fileUploadService = fileUploadService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Get all contracts with optional filtering
        /// </summary>
        /// <param name="status">Filter by contract status</param>
        /// <param name="clientId">Filter by client ID</param>
        /// <param name="startDateFrom">Filter by start date from</param>
        /// <param name="endDateTo">Filter by end date to</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContractResponseDto>>> GetContracts(
            [FromQuery] ContractStatus? status = null,
            [FromQuery] int? clientId = null,
            [FromQuery] DateTime? startDateFrom = null,
            [FromQuery] DateTime? endDateTo = null)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (clientId.HasValue)
                query = query.Where(c => c.ClientId == clientId.Value);

            if (startDateFrom.HasValue)
                query = query.Where(c => c.StartDate >= startDateFrom.Value);

            if (endDateTo.HasValue)
                query = query.Where(c => c.EndDate <= endDateTo.Value);

            var contracts = await query
                .Select(c => new ContractResponseDto
                {
                    Id = c.Id,
                    ClientId = c.ClientId,
                    ClientName = c.Client.Name,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    ServiceLevel = c.ServiceLevel,
                    SignedAgreementPath = c.SignedAgreementPath,
                    OriginalFileName = c.OriginalFileName,
                    IsActive = c.IsActive(),
                    CanCreateServiceRequest = c.CanCreateServiceRequest()
                })
                .ToListAsync();

            return Ok(contracts);
        }

        /// <summary>
        /// Get a specific contract by ID
        /// </summary>
        /// <param name="id">Contract ID</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<ContractResponseDto>> GetContract(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            var response = new ContractResponseDto
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                ClientName = contract.Client?.Name,
                ClientRegion = contract.Client?.Region,
                ClientContactDetails = contract.Client?.ContactDetails,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                ServiceLevel = contract.ServiceLevel,
                SignedAgreementPath = contract.SignedAgreementPath,
                OriginalFileName = contract.OriginalFileName,
                IsActive = contract.IsActive(),
                CanCreateServiceRequest = contract.CanCreateServiceRequest()
            };

            return Ok(response);
        }

        /// <summary>
        /// Create a new contract
        /// </summary>
        /// <param name="createDto">Contract creation data</param>
        [HttpPost]
        public async Task<ActionResult<ContractResponseDto>> CreateContract([FromForm] ContractDtos createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate client exists
            var client = await _context.Clients.FindAsync(createDto.ClientId);
            if (client == null)
                return BadRequest(new { message = $"Client with ID {createDto.ClientId} not found" });

            // Validate dates
            if (createDto.EndDate <= createDto.StartDate)
                return BadRequest(new { message = "End date must be after start date" });

            var contract = new Contract
            {
                ClientId = createDto.ClientId,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Status = createDto.Status,
                ServiceLevel = createDto.ServiceLevel
            };

            // Handle file upload
            if (createDto.SignedAgreement != null && createDto.SignedAgreement.Length > 0)
            {
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", "uploads", "contracts");
                var uploadResult = await _fileUploadService.UploadPdfAsync(createDto.SignedAgreement, uploadFolder);

                if (uploadResult.success)
                {
                    contract.SignedAgreementPath = uploadResult.filePath;
                    contract.OriginalFileName = createDto.SignedAgreement.FileName;
                }
                else
                {
                    return BadRequest(new { message = uploadResult.errorMessage });
                }
            }

            await _contractRepository.AddAsync(contract);

            var response = new ContractResponseDto
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                ClientName = client.Name,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                ServiceLevel = contract.ServiceLevel,
                SignedAgreementPath = contract.SignedAgreementPath,
                OriginalFileName = contract.OriginalFileName,
                IsActive = contract.IsActive(),
                CanCreateServiceRequest = contract.CanCreateServiceRequest()
            };

            return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, response);
        }

        /// <summary>
        /// Update contract status (PATCH)
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="statusDto">New status</param>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateContractStatus(int id, [FromBody] ContractUpdateStatusDto statusDto)
        {
            var contract = await _contractRepository.GetByIdAsync(id);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            contract.Status = statusDto.Status;
            await _contractRepository.UpdateAsync(contract);

            return Ok(new
            {
                message = "Contract status updated successfully",
                id = contract.Id,
                newStatus = contract.Status.ToString()
            });
        }

        /// <summary>
        /// Download the signed agreement PDF for a contract
        /// </summary>
        /// <param name="id">Contract ID</param>
        [HttpGet("{id}/agreement")]
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            if (string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound(new { message = "This contract has no signed agreement." });

            if (!System.IO.File.Exists(contract.SignedAgreementPath))
                return NotFound(new { message = "The agreement file is missing on the server." });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(contract.SignedAgreementPath);
            var fileName = contract.OriginalFileName ?? $"Contract_{id}_Agreement.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Delete a contract
        /// </summary>
        /// <param name="id">Contract ID</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var result = await _contractRepository.DeleteAsync(id);

            if (!result)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            return NoContent(); // 204 - Success with no content
        }
    }
}