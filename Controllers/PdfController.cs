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

    /// <summary>
    /// PDF hochladen (nur für Admin)
    /// POST: /api/pdf/upload
    /// Content-Type: multipart/form-data
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        // Admin-Authentifizierung prüfen
        if (!_pdfService.IsAdminRequest(HttpContext))
        {
            _logger.LogWarning($"❌ Unautorisierter Upload-Versuch von {HttpContext.Connection.RemoteIpAddress}");
            return Unauthorized(new { success = false, message = "Nur Admin darf PDFs hochladen" });
        }

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("❌ Upload ohne Datei");
            return BadRequest(new { success = false, message = "Keine PDF-Datei ausgewählt" });
        }

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning($"❌ Upload mit falscher Content-Type: {file.ContentType}");
            return BadRequest(new { success = false, message = "Nur PDF-Dateien erlaubt" });
        }

        // Max 50 MB
        const long maxSize = 50 * 1024 * 1024;
        if (file.Length > maxSize)
        {
            _logger.LogWarning($"❌ PDF zu groß: {file.Length} bytes");
            return BadRequest(new { success = false, message = "PDF zu groß (Max 50 MB)" });
        }

        try
        {
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                byte[] pdfBytes = ms.ToArray();

                if (!IsPdfContent(pdfBytes))
                {
                    _logger.LogWarning($"❌ Upload ohne PDF-Signatur: {file.FileName}");
                    return BadRequest(new { success = false, message = "Datei ist keine gültige PDF" });
                }

                if (_pdfService.SavePdfContent(pdfBytes))
                {
                    _logger.LogInformation($"✅ Admin hat PDF hochgeladen: {file.FileName} ({pdfBytes.Length} bytes)");
                    return Ok(new 
                    { 
                        success = true, 
                        message = "PDF erfolgreich hochgeladen",
                        size = pdfBytes.Length,
                        fileName = file.FileName
                    });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Fehler beim Speichern der PDF" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Fehler beim PDF-Upload: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Fehler beim Upload" });
        }
    }

    /// <summary>
    /// PDF löschen (nur für Admin)
    /// DELETE: /api/pdf/delete
    /// </summary>
    [HttpDelete("delete")]
    public IActionResult Delete()
    {
        // Admin-Authentifizierung prüfen
        if (!_pdfService.IsAdminRequest(HttpContext))
        {
            _logger.LogWarning($"❌ Unautorisierter Delete-Versuch von {HttpContext.Connection.RemoteIpAddress}");
            return Unauthorized(new { success = false, message = "Nur Admin darf PDFs löschen" });
        }

        try
        {
            if (_pdfService.DeletePdfContent())
            {
                _logger.LogInformation("✅ Admin hat PDF gelöscht");
                return Ok(new { success = true, message = "PDF erfolgreich gelöscht" });
            }
            else
            {
                return StatusCode(500, new { success = false, message = "Fehler beim Löschen der PDF" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Fehler beim PDF-Delete: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Fehler beim Löschen" });
        }
    }

    private static bool IsPdfContent(byte[] content)
    {
        return content.Length >= 5
            && content[0] == '%'
            && content[1] == 'P'
            && content[2] == 'D'
            && content[3] == 'F'
            && content[4] == '-';
    }
}
