using Microsoft.AspNetCore.Mvc;
using CVWebsite.Models;
using CVWebsite.Services;

namespace CVWebsite.Controllers;

/// <summary>
/// Controller für Authentifizierung
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Login-Endpoint
    /// POST: /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Input Validation
        if (request == null || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login-Versuch mit leerem Passwort");
            return BadRequest(new LoginResponse 
            { 
                Success = false, 
                Message = "Passwort erforderlich" 
            });
        }

        // Passwort begrenzen (Schutz vor sehr großen Payloads)
        if (request.Password.Length > 1000)
        {
            _logger.LogWarning("Login-Versuch mit zu langem Passwort");
            return BadRequest(new LoginResponse 
            { 
                Success = false, 
                Message = "Ungültiges Passwort" 
            });
        }

        // Authentifizierung prüfen
        if (_authService.ValidatePassword(request.Password))
        {
            // Session setzen (CSRF-geschützt durch ASP.NET Core)
            HttpContext.Session.SetString("IsAuthenticated", "true");
            
            _logger.LogInformation("Erfolgreicher Login");
            
            return Ok(new LoginResponse 
            { 
                Success = true, 
                Message = "Login erfolgreich" 
            });
        }

        // Fehler nicht spezifisch machen (verhindert User-Enumeration)
        _logger.LogWarning("Fehlgeschlagener Login");
        return Unauthorized(new LoginResponse 
        { 
            Success = false, 
            Message = "Ungültiges Passwort" 
        });
    }

    /// <summary>
    /// Logout-Endpoint
    /// POST: /api/auth/logout
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logout erfolgreich" });
    }

    /// <summary>
    /// Status prüfen
    /// GET: /api/auth/status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated") == "true";
        return Ok(new { isAuthenticated });
    }
}
