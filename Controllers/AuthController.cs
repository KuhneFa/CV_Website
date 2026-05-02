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

    /// <summary>
    /// Admin: PDF hochladen (nur mit Admin-Passwort)
    /// POST: /api/auth/upload
    /// Content-Type: multipart/form-data
    /// Body: file, adminPassword
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadPdf([FromForm] IFormFile file, [FromForm] string adminPassword)
    {
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            _logger.LogWarning("Upload-Versuch ohne Admin-Passwort");
            return Unauthorized(new { success = false, message = "Admin-Passwort erforderlich" });
        }

        if (!_authService.ValidateAdminPassword(adminPassword))
        {
            _logger.LogWarning("Upload mit falschemAdmin-Passwort");
            return Unauthorized(new { success = false, message = "Falsches Admin-Passwort" });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Keine PDF-Datei ausgewählt" });
        }

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { success = false, message = "Nur PDF-Dateien erlaubt" });
        }

        try
        {
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                byte[] pdfBytes = ms.ToArray();
                
                if (pdfBytes.Length > 50 * 1024 * 1024) // 50 MB Limit
                {
                    return BadRequest(new { success = false, message = "PDF zu groß (Max 50 MB)" });
                }

                string base64Pdf = Convert.ToBase64String(pdfBytes);
                
                // TODO: Speichere PDF persistenter (z.B. in Datei oder DB)
                // Für jetzt speichern wir es nur im Memory
                
                _logger.LogInformation($"✅ Admin hat PDF hochgeladen ({pdfBytes.Length} bytes, {file.FileName})");
                return Ok(new { success = true, message = "PDF erfolgreich hochgeladen", size = pdfBytes.Length });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fehler beim PDF-Upload: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Fehler beim Upload" });
        }
    }
}
