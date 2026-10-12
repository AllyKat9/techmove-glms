using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS_Web.Data.DTOs;
using TechMoveGLMS_Web.Models;
using TechMoveGLMS_Web.Services;

namespace TechMoveGLMS_Web.Controllers
{
    [Authorize] // Added authorization to protect contract endpoints
    public class ContractsController : Controller
    {
        private readonly IContractApiService _contractApiService;
        private readonly IClientApiService _clientApiService;
        private readonly IServiceRequestApiService _serviceRequestApiService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            IContractApiService contractApiService,
            IClientApiService clientApiService,
            IServiceRequestApiService serviceRequestApiService,
            ILogger<ContractsController> logger)
        {
            _contractApiService = contractApiService;
            _clientApiService = clientApiService;
            _serviceRequestApiService = serviceRequestApiService;
            _logger = logger;
        }

        // Displays list of all contracts with optional filtering
        public async Task<IActionResult> Index(ContractStatus? status = null, int? clientId = null)
        {
            try
            {
                _logger.LogInformation("Loading contracts with filters - Status: {Status}, ClientId: {ClientId}", status, clientId);

                var contracts = await _contractApiService.GetContractsAsync(status, clientId);

                // Populate filter options for the view
                ViewBag.SelectedStatus = status;
                ViewBag.SelectedClientId = clientId;
                ViewBag.Clients = await _clientApiService.GetClientsAsync();

                return View(contracts ?? new List<ContractResponseDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contracts with filters - Status: {Status}, ClientId: {ClientId}", status, clientId);
                TempData["Error"] = "Unable to load contracts. API service may be unavailable. Please try again later.";
                return View(new List<ContractResponseDto>());
            }
        }

        // Displays details for a specific contract
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("Loading contract details for ID: {ContractId}", id);
                var contract = await _contractApiService.GetContractByIdAsync(id);

                if (contract == null)
                {
                    _logger.LogWarning("Contract not found with ID: {ContractId}", id);
                    return NotFound($"Contract with ID {id} was not found.");
                }

                var allRequests = await _serviceRequestApiService.GetServiceRequestsAsync();
                ViewBag.ServiceRequests = allRequests.Where(r => r.ContractId == id).ToList();

                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contract details for ID: {ContractId}", id);
                TempData["Error"] = "Unable to load contract details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Returns create contract form with clients dropdown
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Loading contract creation form");
                var clients = await _clientApiService.GetClientsAsync();
                ViewBag.Clients = clients;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contract creation form");
                TempData["Error"] = "Unable to load the contract creation form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Creates a new contract with optional file upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractCreateDto contract, IFormFile? SignedAgreement = null)
        {
            try
            {
                // Remove navigation properties from validation
                ModelState.Remove("Client");
                ModelState.Remove("ServiceRequests");

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Creating new contract for ClientId: {ClientId}", contract.ClientId);

                    // Create contract via API service with file upload if provided
                    var createdContract = await _contractApiService.CreateContractAsync(contract, SignedAgreement);

                    if (createdContract != null)
                    {
                        _logger.LogInformation("Contract created successfully with ID: {ContractId}", createdContract.Id);
                        TempData["Success"] = $"Contract #{createdContract.Id} created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create contract. API returned no response.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract for ClientId: {ClientId}", contract.ClientId);
                ModelState.AddModelError("", $"Failed to create contract: {ex.Message}");
            }

            // Reload clients for the dropdown on error
            try
            {
                var clients = await _clientApiService.GetClientsAsync();
                ViewBag.Clients = clients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading clients for contract creation form");
                ViewBag.Clients = new List<Client>();
            }

            return View();
        }

        // Updates contract status to Active
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                _logger.LogInformation("Activating contract with ID: {ContractId}", id);

                var result = await _contractApiService.UpdateContractStatusAsync(id, ContractStatus.Active);

                if (result)
                {
                    _logger.LogInformation("Contract {ContractId} activated successfully", id);
                    TempData["Success"] = "Contract activated successfully!";
                }
                else
                {
                    _logger.LogWarning("Failed to activate contract {ContractId}", id);
                    TempData["Error"] = "Unable to activate contract. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating contract {ContractId}", id);
                TempData["Error"] = "Unable to activate contract due to a system error.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Updates contract status to On Hold
        public async Task<IActionResult> OnHold (int id)
        {
            try
            {
                _logger.LogInformation("Completing contract with ID: {ContractId}", id);

                var result = await _contractApiService.UpdateContractStatusAsync(id, ContractStatus.OnHold);

                if (result)
                {
                    _logger.LogInformation("Contract {ContractId} placed on hold successfully", id);
                    TempData["Success"] = "Contract placed on hold!";
                }
                else
                {
                    _logger.LogWarning("Failed to place contract on hold {ContractId}", id);
                    TempData["Error"] = "Unable to place contract on hold. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing contract on hold {ContractId}", id);
                TempData["Error"] = "Unable to place contract on hold due to a system error.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Deletes a contract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting contract with ID: {ContractId}", id);

                var result = await _contractApiService.DeleteContractAsync(id);

                if (result)
                {
                    _logger.LogInformation("Contract {ContractId} deleted successfully", id);
                    TempData["Success"] = "Contract deleted successfully!";
                }
                else
                {
                    _logger.LogWarning("Failed to delete contract {ContractId}", id);
                    TempData["Error"] = "Unable to delete contract. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {ContractId}", id);
                TempData["Error"] = "Unable to delete contract due to a system error.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Searches and filters contracts by date range and status
        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            try
            {
                _logger.LogInformation("Searching contracts - StartDate: {StartDate}, EndDate: {EndDate}, Status: {Status}",
                    startDate, endDate, status);

                // Get all contracts first (API might not support date filtering)
                var allContracts = await _contractApiService.GetContractsAsync(status, null);

                // Apply date filtering locally
                var filteredContracts = allContracts?.Where(c =>
                    (!startDate.HasValue || c.StartDate >= startDate.Value) &&
                    (!endDate.HasValue || c.EndDate <= endDate.Value)
                ).ToList() ?? new List<ContractResponseDto>();

                // Pass filter parameters to view for display
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.SelectedStatus = status;

                _logger.LogInformation("Search returned {Count} contracts", filteredContracts.Count);

                return View("Search", filteredContracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching contracts with date range");
                TempData["Error"] = "Unable to search contracts. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Downloads the signed agreement PDF for a contract
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            try
            {
                _logger.LogInformation("Downloading agreement for contract ID: {ContractId}", id);

                var result = await _contractApiService.DownloadAgreementAsync(id);

                if (result == null)
                {
                    TempData["Error"] = "The agreement file could not be downloaded.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return File(result.FileBytes, result.ContentType, result.OriginalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading agreement for contract ID: {ContractId}", id);
                TempData["Error"] = "Unable to download the agreement file. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Edits contract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractStatus status, ServiceLevel? serviceLevel = null)
        {
            try
            {
                _logger.LogInformation("Editing contract {ContractId} - New Status: {Status}, ServiceLevel: {ServiceLevel}",
                    id, status, serviceLevel);

                // Update status
                var statusUpdated = await _contractApiService.UpdateContractStatusAsync(id, status);

                if (!statusUpdated)
                {
                    TempData["Error"] = "Failed to update contract status.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["Success"] = "Contract updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing contract {ContractId}", id);
                TempData["Error"] = "Unable to update contract due to a system error.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}