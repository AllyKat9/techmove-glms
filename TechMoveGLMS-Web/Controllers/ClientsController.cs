using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS_Web.Models;
using TechMoveGLMS_Web.Services;


namespace TechMoveGLMS_Web.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly IClientApiService _clientApiService;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IClientApiService clientApiService, ILogger<ClientsController> logger)
        {
            _clientApiService = clientApiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var clients = await _clientApiService.GetClientsAsync();
                return View(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clients");
                TempData["Error"] = "Unable to load clients. API may be unavailable.";
                return View(new List<Client>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var client = await _clientApiService.GetClientByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client {Id}", id);
                return NotFound();
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ContactDetails,Region")] Client client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _clientApiService.CreateClientAsync(client);
                    TempData["Success"] = "Client created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating client");
                    ModelState.AddModelError("", $"Failed to create client: {ex.Message}");
                }
            }
            return View(client);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var client = await _clientApiService.GetClientByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return View(client);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactDetails,Region")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _clientApiService.UpdateClientAsync(client);
                    TempData["Success"] = "Client updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating client {Id}", id);
                    ModelState.AddModelError("", $"Failed to update client: {ex.Message}");
                }
            }
            return View(client);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = await _clientApiService.GetClientByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return View(client);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _clientApiService.DeleteClientAsync(id);
                if (result)
                {
                    TempData["Success"] = "Client deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {Id}", id);
                TempData["Error"] = "Unable to delete client. They may have existing contracts.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
