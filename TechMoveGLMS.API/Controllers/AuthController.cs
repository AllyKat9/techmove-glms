using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TechMoveGLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            // Demo credentials read from configuration.
            // In production, replace with ASP.NET Identity + hashed passwords + DB-backed users.
            var demoUsername = _configuration["DemoCredentials:Username"];
            var demoPassword = _configuration["DemoCredentials:Password"];

            if (string.IsNullOrEmpty(demoUsername ) || string.IsNullOrEmpty(demoPassword))
            {
                _logger.LogError("Demo credentials are not configured properly.");
                return StatusCode(500, new { message = "Server authentication is not configured." });
            }

            if (request.Username == demoUsername && request.Password == demoPassword)
            {
                var token = GenerateJwtToken(request.Username);
                _logger.LogInformation("User {Username} logged in successfully", request.Username);

                return Ok(new LoginResponse
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    Username = request.Username
                });
            }

            _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        private string GenerateJwtToken(string username)
        {
            // All JWT config is validated at startup in Program.cs, so we can safely
            // access it via pointer. The null-operator (!) is intentional.

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; }
    }
}
