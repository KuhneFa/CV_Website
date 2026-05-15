using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using CVWebsite.Models;
using CVWebsite.Services;

namespace CVWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILoginAttemptService _loginAttemptService;
    private readonly IAntiforgery _antiforgery;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _env;

    public AuthController(
        IAuthService authService,
        ILoginAttemptService loginAttemptService,
        IAntiforgery antiforgery,
        ILogger<AuthController> logger,
        IWebHostEnvironment env)
    {
        _authService = authService;
        _loginAttemptService = loginAttemptService;
        _antiforgery = antiforgery;
        _logger = logger;
        _env = env;
    }

    [HttpPost("login-auto")]
    public async Task<IActionResult> LoginAuto([FromBody] LoginRequest request)
    {
        if (!await IsCsrfValid())
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Ungültige Anfrage"
            });
        }

        if (request == null || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Auto-Login-Versuch mit leerem Passwort");
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Passwort erforderlich"
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            _logger.LogWarning("Bot-verdächtiger Auto-Login-Versuch über Honeypot-Feld");
            _loginAttemptService.RegisterFailedAttempt(GetLoginAttemptKey("auto"));
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Ungültige Anfrage"
            });
        }

        if (request.Password.Length > 1000)
        {
            _logger.LogWarning("Auto-Login-Versuch mit zu langem Passwort");
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Ungültiges Passwort"
            });
        }

        var adminAttemptKey = GetLoginAttemptKey("admin");
        var userAttemptKey = GetLoginAttemptKey("user");

        var isAdminLocked = _loginAttemptService.IsLocked(adminAttemptKey, out var adminRemaining);
        var isUserLocked = _loginAttemptService.IsLocked(userAttemptKey, out var userRemaining);
        if (isAdminLocked || isUserLocked)
        {
            var remaining = adminRemaining > userRemaining ? adminRemaining : userRemaining;
            _logger.LogWarning($"Auto-Login temporär gesperrt für {HttpContext.Connection.RemoteIpAddress}");
            return StatusCode(StatusCodes.Status429TooManyRequests, new LoginResponse
            {
                Success = false,
                Message = $"Zu viele Versuche. Bitte in {Math.Ceiling(remaining.TotalMinutes)} Minuten erneut versuchen."
            });
        }

        if (_authService.ValidateAdminPassword(request.Password))
        {
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("IsAdmin", "true");
            _loginAttemptService.Reset(adminAttemptKey);
            _loginAttemptService.Reset(userAttemptKey);
            _logger.LogInformation("✅ Erfolgreicher Auto-Login als Admin");
            return Ok(new
            {
                success = true,
                message = "Admin-Login erfolgreich",
                role = "admin"
            });
        }

        if (_authService.ValidatePassword(request.Password))
        {
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.Remove("IsAdmin");
            _loginAttemptService.Reset(adminAttemptKey);
            _loginAttemptService.Reset(userAttemptKey);
            _logger.LogInformation("Erfolgreicher Auto-Login als User");
            return Ok(new
            {
                success = true,
                message = "Login erfolgreich",
                role = "user"
            });
        }

        _logger.LogWarning("Fehlgeschlagener Auto-Login");
        _loginAttemptService.RegisterFailedAttempt(adminAttemptKey);
        _loginAttemptService.RegisterFailedAttempt(userAttemptKey);
        return Ok(new
        {
            success = false,
            message = "Ungültiges Passwort",
            role = ""
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!await IsCsrfValid())
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Ungültige Anfrage"
            });
        }

        var attemptKey = GetLoginAttemptKey("user");
        if (_loginAttemptService.IsLocked(attemptKey, out var remaining))
        {
            _logger.LogWarning($"Login temporär gesperrt für {HttpContext.Connection.RemoteIpAddress}");
            return StatusCode(StatusCodes.Status429TooManyRequests, new LoginResponse
            {
                Success = false,
                Message = $"Zu viele Versuche. Bitte in {Math.Ceiling(remaining.TotalMinutes)} Minuten erneut versuchen."
            });
        }

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
            _loginAttemptService.RegisterFailedAttempt(attemptKey);
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
            _loginAttemptService.Reset(attemptKey);
            _logger.LogInformation("Erfolgreicher Login");
            return Ok(new LoginResponse 
            { 
                Success = true, 
                Message = "Login erfolgreich" 
            });
        }

        _logger.LogWarning("Fehlgeschlagener Login");
        _loginAttemptService.RegisterFailedAttempt(attemptKey);
        return Unauthorized(new LoginResponse 
        { 
            Success = false, 
            Message = "Ungültiges Passwort" 
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (!await IsCsrfValid())
        {
            return BadRequest(new { message = "Ungültige Anfrage" });
        }

        HttpContext.Session.Clear();
        return Ok(new { message = "Logout erfolgreich" });
    }

    /// <summary>
    /// Admin-Login (anderes Passwort als Public-User)
    /// POST: /api/auth/admin-login
    /// </summary>
    [HttpPost("admin-login")]
    public async Task<IActionResult> AdminLogin([FromBody] LoginRequest request)
    {
        if (!await IsCsrfValid())
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Ungültige Anfrage"
            });
        }

        var attemptKey = GetLoginAttemptKey("admin");
        if (_loginAttemptService.IsLocked(attemptKey, out var remaining))
        {
            _logger.LogWarning($"Admin-Login temporär gesperrt für {HttpContext.Connection.RemoteIpAddress}");
            return StatusCode(StatusCodes.Status429TooManyRequests, new LoginResponse
            {
                Success = false,
                Message = $"Zu viele Versuche. Bitte in {Math.Ceiling(remaining.TotalMinutes)} Minuten erneut versuchen."
            });
        }

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
            _loginAttemptService.RegisterFailedAttempt(attemptKey);
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
            _loginAttemptService.Reset(attemptKey);
            _logger.LogInformation("✅ Erfolgreicher Admin-Login");
            return Ok(new LoginResponse 
            { 
                Success = true, 
                Message = "Admin-Login erfolgreich" 
            });
        }

        _logger.LogWarning("⚠️ Fehlgeschlagener Admin-Login");
        _loginAttemptService.RegisterFailedAttempt(attemptKey);
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

    private string GetLoginAttemptKey(string scope)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"login-attempt:{scope}:{ip}";
    }

    private async Task<bool> IsCsrfValid()
    {
        try
        {
            await _antiforgery.ValidateRequestAsync(HttpContext);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            _logger.LogWarning($"Ungültiger CSRF-Token von {HttpContext.Connection.RemoteIpAddress}");
            return false;
        }
    }
}
