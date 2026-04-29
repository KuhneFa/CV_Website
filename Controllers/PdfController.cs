using Microsoft.AspNetCore.Mvc;
using CVWebsite.Services;

namespace CVWebsite.Controllers;

/// <summary>
/// Controller für PDF-Download (nur für authentifizierte User)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly IPdfService _pdfService;
    private readonly ILogger<PdfController> _logger;

    public PdfController(IPdfService pdfService, ILogger<PdfController> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// PDF downloaden (nur wenn authentifiziert)
    /// GET: /api/pdf/download
    /// </summary>
    [HttpGet("download")]
    public IActionResult Download()
    {
        // Authentifizierung prüfen
        if (!_pdfService.IsValidRequest(HttpContext))
        {
            _logger.LogWarning($"Unauthentifizierter PDF-Zugriff von {HttpContext.Connection.RemoteIpAddress}");
            return Unauthorized(new { message = "Authentifizierung erforderlich" });
        }

        // PDF laden
        byte[]? pdfContent = _pdfService.GetPdfContent();
        if (pdfContent == null || pdfContent.Length == 0)
        {
            _logger.LogError("PDF konnte nicht geladen werden");
            return StatusCode(500, new { message = "PDF nicht verfügbar" });
        }

        // PDF mit sicheren Headers zurückgeben
        return File(
            pdfContent,
            "application/pdf",
            "Lebenslauf.pdf",
            enableRangeProcessing: true
        );
    }

    /// <summary>
    /// PDF-Status prüfen
    /// GET: /api/pdf/status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        bool hasAccess = _pdfService.IsValidRequest(HttpContext);
        return Ok(new { hasAccess });
    }
}
