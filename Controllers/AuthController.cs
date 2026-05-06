using Microsoft.AspNetCore.Mvc;
using CVWebsite.Models;
using CVWebsite.Services;

namespace CVWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, IWebHostEnvironment env)
    {
        _authService = authService;
        _logger = logger;
        _env = env;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login-Versuch mit leerem Passwort");
            return BadRequest(new LoginResponse 
            { 
                Success = false, 
                Message = "Passwort erforderlich" 
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            _logger.LogWarning("Bot-verdächtiger Login-Versuch über Honeypot-Feld");
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Ungültige Anfrage"
            });
        }

        if (request.Password.Length > 1000)
        {
            _logger.LogWarning("Login-Versuch mit zu langem Passwort");
            return BadRequest(new LoginResponse 
            { 
                Success = false, 
                Message = "Ungültiges Passwort" 
            });
        }

        if (_authService.ValidatePassword(request.Password))
        {
            HttpContext.Session.SetString("IsAuthenticated", "true");
            _logger.LogInformation("Erfolgreicher Login");
            return Ok(new LoginResponse 
            { 
                Success = true, 
                Message = "Login erfolgreich" 
            });
        }

        _logger.LogWarning("Fehlgeschlagener Login");
        return Unauthorized(new LoginResponse 
        { 
            Success = false, 
            Message = "Ungültiges Passwort" 
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logout erfolgreich" });
    }

    /// <summary>
    /// Admin-Login (anderes Passwort als Public-User)
    /// POST: /api/auth/admin-login
    /// </summary>
    [HttpPost("admin-login")]
    public IActionResult AdminLogin([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Admin-Login-Versuch mit leerem Passwort");
            return BadRequest(new LoginResponse 
            { 
                Success = false, 
                Message = "Admin-Passwort erforderlich" 
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            _logger.LogWarning("Bot-verdächtiger Admin-Login-Versuch über Honeypot-Feld");
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Ungültige Anfrage"
            });
        }

        if (request.Password.Length > 1000)
        {
            _logger.LogWarning("Admin-Login-Versuch mit zu langem Passwort");
            return BadRequest(new LoginResponse 
            { 
                Success = false, 
                Message = "Ungültiges Passwort" 
            });
        }

        if (_authService.ValidateAdminPassword(request.Password))
        {
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("IsAdmin", "true");
            _logger.LogInformation("✅ Erfolgreicher Admin-Login");
            return Ok(new LoginResponse 
            { 
                Success = true, 
                Message = "Admin-Login erfolgreich" 
            });
        }

        _logger.LogWarning("⚠️ Fehlgeschlagener Admin-Login");
        return Unauthorized(new LoginResponse 
        { 
            Success = false, 
            Message = "Falsches Admin-Passwort" 
        });
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated") == "true";
        return Ok(new { isAuthenticated });
    }

    [HttpGet("generate-hash")]
    public IActionResult GenerateHash([FromQuery] string password)
    {
        if (!_env.IsDevelopment())
        {
            return Unauthorized(new { message = "Nur im Development verfügbar" });
        }

        if (string.IsNullOrEmpty(password))
        {
            return BadRequest(new { message = "Passwort erforderlich" });
        }

        string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
        return Ok(new { password, hash });
    }

}
