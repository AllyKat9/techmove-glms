using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.API.DTOs;
using TechMoveGLMS_Web.Data;
using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientResponseDto>>> GetClients()
        {
            var clients = await _context.Clients
                .Include(c => c.Contracts)
                .Select(c => new ClientResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactDetails = c.ContactDetails,
                    Region = c.Region,
                    ContractCount = c.Contracts.Count
                })
                .ToListAsync();

            return Ok(clients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientResponseDto>> GetClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound(new { message = $"Client with ID {id} not found" });

            var response = new ClientResponseDto
            {
                Id = client.Id,
                Name = client.Name,
                ContactDetails = client.ContactDetails,
                Region = client.Region,
                ContractCount = client.Contracts.Count
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ClientResponseDto>> CreateClient([FromBody] ClientCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = new Client
            {
                Name = createDto.Name,
                ContactDetails = createDto.ContactDetails,
                Region = createDto.Region
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var response = new ClientResponseDto
            {
                Id = client.Id,
                Name = client.Name,
                ContactDetails = client.ContactDetails,
                Region = client.Region,
                ContractCount = 0
            };

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientUpdateDto updateDto)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound(new { message = $"Client with ID {id} not found" });

            client.Name = updateDto.Name;
            client.ContactDetails = updateDto.ContactDetails;
            client.Region = updateDto.Region;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Client updated successfully", id = client.Id });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound(new { message = $"Client with ID {id} not found" });

            if (client.Contracts.Any())
                return BadRequest(new { message = "Cannot delete client with existing contracts" });

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
