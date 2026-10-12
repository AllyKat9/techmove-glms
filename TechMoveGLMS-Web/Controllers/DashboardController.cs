using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechMoveGLMS_Web.Services;
using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS_Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IContractApiService _contractApiService;
        private readonly IClientApiService _clientApiService;
        private readonly IServiceRequestApiService _serviceRequestApiService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IContractApiService contractApiService,
            IClientApiService clientApiService,
            IServiceRequestApiService serviceRequestApiService,
            ILogger<DashboardController> logger)
        {
            _contractApiService = contractApiService;
            _clientApiService = clientApiService;
            _serviceRequestApiService = serviceRequestApiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var clients = await _clientApiService.GetClientsAsync();
                var contracts = await _contractApiService.GetContractsAsync();
                var serviceRequests = await _serviceRequestApiService.GetServiceRequestsAsync();

                ViewBag.TotalClients = clients?.Count ?? 0;
                ViewBag.TotalContracts = contracts?.Count ?? 0;
                ViewBag.ActiveContracts = contracts?.Count(c => c.Status == ContractStatus.Active) ?? 0;
                ViewBag.TotalServiceRequests = serviceRequests?.Count ?? 0;
                ViewBag.PendingRequests = serviceRequests?.Count(r => r.Status == RequestStatus.Pending) ?? 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                ViewBag.TotalClients = 0;
                ViewBag.TotalContracts = 0;
                ViewBag.ActiveContracts = 0;
                ViewBag.TotalServiceRequests = 0;
                ViewBag.PendingRequests = 0;

                TempData["Error"] = "Unable to load dashboard data. API may be unavailable.";
                return View();
            }
        }
    }
}
