namespace CVWebsite.Services;

/// <summary>
/// Service für PDF-Handling & Sicherheit
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Gibt die PDF als Byte-Array zurück
    /// </summary>
    byte[]? GetPdfContent();
    
    /// <summary>
    /// Validiert, ob die Anfrage vertrauenswürdig ist
    /// </summary>
    bool IsValidRequest(HttpContext context);
}

public class PdfService : IPdfService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PdfService> _logger;

    public PdfService(IConfiguration configuration, ILogger<PdfService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// PDF aus Base64-String aus Umgebungsvariable laden
    /// </summary>
    public byte[]? GetPdfContent()
    {
        try
        {
            var pdfBase64 = _configuration["Pdf:ContentBase64"];
            
            if (string.IsNullOrEmpty(pdfBase64))
            {
                _logger.LogError("PDF nicht in Konfiguration gefunden!");
                return null;
            }

            byte[] pdfBytes = Convert.FromBase64String(pdfBase64);
            _logger.LogInformation($"PDF geladen ({pdfBytes.Length} bytes)");
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fehler beim Laden der PDF: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Prüft ob Request vertrauenswürdig ist (z.B. Session vorhanden)
    /// </summary>
    public bool IsValidRequest(HttpContext context)
    {
        // Prüfe ob User authentifiziert ist (aus Session)
        var isAuthenticated = context.Session.GetString("IsAuthenticated") == "true";
        
        if (!isAuthenticated)
        {
            _logger.LogWarning($"Unauthentifizierter PDF-Zugriff von {context.Connection.RemoteIpAddress}");
        }

        return isAuthenticated;
    }
}
