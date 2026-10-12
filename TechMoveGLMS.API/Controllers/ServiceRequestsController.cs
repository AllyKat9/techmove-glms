using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.API.DTOs;
using TechMoveGLMS_Web.Data;
using TechMoveGLMS.API.Services;

namespace TechMoveGLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ServiceRequestFactory _serviceRequestFactory;
        private readonly ICurrencyConverter _currencyConverter;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ServiceRequestFactory serviceRequestFactory,
            ICurrencyConverter currencyConverter)
        {
            _context = context;
            _serviceRequestFactory = serviceRequestFactory;
            _currencyConverter = currencyConverter;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestResponseDto>>> GetServiceRequests()
        {
            var requests = await _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c.Client)
                .Select(r => new ServiceRequestResponseDto
                {
                    Id = r.Id,
                    ContractId = r.ContractId,
                    ContractServiceLevel = r.Contract.ServiceLevel.ToString(),
                    ClientName = r.Contract.Client.Name,
                    Description = r.Description,
                    CostUSD = r.CostUSD,
                    CostZAR = r.CostZAR,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ExchangeRateUsed = r.ExchangeRateUsed
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequestResponseDto>> GetServiceRequest(int id)
        {
            var request = await _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound(new { message = $"Service request with ID {id} not found" });

            var response = new ServiceRequestResponseDto
            {
                Id = request.Id,
                ContractId = request.ContractId,
                ContractServiceLevel = request.Contract.ServiceLevel.ToString(),
                ClientName = request.Contract.Client.Name,
                Description = request.Description,
                CostUSD = request.CostUSD,
                CostZAR = request.CostZAR,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                ExchangeRateUsed = request.ExchangeRateUsed
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceRequestResponseDto>> CreateServiceRequest([FromBody] ServiceRequestCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get current exchange rate
            var exchangeRate = await _currencyConverter.GetExchangeRate("USD", "ZAR");

            var result = await _serviceRequestFactory.CreateRequestAsync(
                createDto.ContractId,
                createDto.Description,
                createDto.CostUSD,
                exchangeRate);

            if (!result.success)
                return BadRequest(new { message = result.errorMessage });

            _context.ServiceRequests.Add(result.request);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(result.request)
                .Reference(r => r.Contract)
                .Query()
                .Include(c => c.Client)
                .LoadAsync();

            var response = new ServiceRequestResponseDto
            {
                Id = result.request.Id,
                ContractId = result.request.ContractId,
                ContractServiceLevel = result.request.Contract.ServiceLevel.ToString(),
                ClientName = result.request.Contract.Client.Name,
                Description = result.request.Description,
                CostUSD = result.request.CostUSD,
                CostZAR = result.request.CostZAR,
                Status = result.request.Status,
                CreatedAt = result.request.CreatedAt,
                ExchangeRateUsed = result.request.ExchangeRateUsed
            };

            return CreatedAtAction(nameof(GetServiceRequest), new { id = result.request.Id }, response);
        }

        [HttpGet("exchange-rate")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExchangeRate()
        {
            var rate = await _currencyConverter.GetExchangeRate("USD", "ZAR");
            return Ok(new { fromCurrency = "USD", toCurrency = "ZAR", rate = rate });
        }
    }
}