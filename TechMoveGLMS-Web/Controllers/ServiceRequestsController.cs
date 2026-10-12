using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechMoveGLMS_Web.Data.DTOs;
using TechMoveGLMS_Web.Models;
using TechMoveGLMS_Web.Services;

namespace TechMoveGLMS_Web.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly IServiceRequestApiService _serviceRequestApiService;
        private readonly IContractApiService _contractApiService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            IServiceRequestApiService serviceRequestApiService,
            IContractApiService contractApiService,
            ILogger<ServiceRequestsController> logger)
        {
            _serviceRequestApiService = serviceRequestApiService;
            _contractApiService = contractApiService;
            _logger = logger;
        }

        // Displays a list of all service requests with related contract and client
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading service requests index");
                var requests = await _serviceRequestApiService.GetServiceRequestsAsync();
                return View(requests ?? new List<ServiceRequest>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service requests");
                TempData["Error"] = "Unable to load service requests. API may be unavailable.";

                return View(new List<ServiceRequest>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Loading service request creation form");

                // Get active contracts from API
                var activeContracts = await _contractApiService.GetContractsAsync(ContractStatus.Active);

                if (activeContracts == null || !activeContracts.Any())
                {
                    _logger.LogWarning("No active contracts found");
                    TempData["Warning"] = "No active contracts available. Please create an active contract first.";
                    ViewBag.Contracts = new List<ContractResponseDto>();
                }
                else
                {
                    _logger.LogInformation("Found {Count} active contracts", activeContracts.Count);
                    ViewBag.Contracts = activeContracts;
                }

                // Get current exchange rate
                try
                {
                    var exchangeRate = await _serviceRequestApiService.GetExchangeRateAsync();

                    ViewBag.ExchangeRate = exchangeRate;
                    _logger.LogInformation("Exchange rate loaded: {Rate}", exchangeRate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading exchange rate, using fallback");
                    ViewBag.ExchangeRate = 19.50m;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service request creation form");
                TempData["Error"] = "Unable to load the service request form. Please make sure the API is running.";
                ViewBag.Contracts = new List<Contract>();
                ViewBag.ExchangeRate = 19.50m;

                return View();
            }
        }

        // Create a new service request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int contractId, string description, decimal costUSD)
        {
            try
            {
                _logger.LogInformation("Creating service request for ContractId: {ContractId}, CostUSD: {CostUSD}", contractId, costUSD);

                var request = await _serviceRequestApiService.CreateServiceRequestAsync(contractId, description, costUSD);

                TempData["Success"] = "Service request created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request for contract {ContractId}", contractId);
                ModelState.AddModelError("", $"Failed to create service request: {ex.Message}");

                // Reload data for the form on error
                try
                {
                    var activeContracts = await _contractApiService.GetContractsAsync(ContractStatus.Active);
                    ViewBag.Contracts = activeContracts ?? new List<ContractResponseDto>();

                    var exchangeRate = await _serviceRequestApiService.GetExchangeRateAsync();
                    ViewBag.ExchangeRate = exchangeRate;
                }
                catch (Exception reloadEx)
                {
                    _logger.LogError(reloadEx, "Error reloading form data");
                    ViewBag.Contracts = new List<Contract>();
                    ViewBag.ExchangeRate = 19.50m;
                }

                return View();
            }
        }

        // Get the current exchange rate for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetExchangeRate()
        {
            try
            {
                var rate = await _serviceRequestApiService.GetExchangeRateAsync();
                return Json(new { rate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate");
                return Json(new { rate = 19.50m, error = "Using fallback rate" });
            }
        }
    }
}