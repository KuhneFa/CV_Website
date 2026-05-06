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
    /// Validiert, ob die Anfrage vertrauenswürdig ist (User authentifiziert)
    /// </summary>
    bool IsValidRequest(HttpContext context);

    /// <summary>
    /// Speichert eine PDF (nur für Admin)
    /// </summary>
    bool SavePdfContent(byte[] pdfBytes);

    /// <summary>
    /// Löscht die aktuelle PDF (nur für Admin)
    /// </summary>
    bool DeletePdfContent();

    /// <summary>
    /// Prüft ob Admin authentifiziert ist
    /// </summary>
    bool IsAdminRequest(HttpContext context);
}

public class PdfService : IPdfService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PdfService> _logger;
    private readonly string _pdfStoragePath;
    
    // In-Memory Fallback für PDF (wenn nicht in Datei gespeichert)
    private static byte[]? _pdfInMemory;

    public PdfService(IConfiguration configuration, ILogger<PdfService> logger, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _logger = logger;
        _pdfStoragePath = Path.Combine(env.ContentRootPath, "data", "cv.pdf");
        
        // Erstelle data-Verzeichnis wenn nicht vorhanden
        var directory = Path.GetDirectoryName(_pdfStoragePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }
    }

    /// <summary>
    /// PDF aus Datei oder Memory laden
    /// </summary>
    public byte[]? GetPdfContent()
    {
        try
        {
            // Versuche zuerst aus Datei zu laden
            if (File.Exists(_pdfStoragePath))
            {
                byte[] pdfBytes = File.ReadAllBytes(_pdfStoragePath);
                _logger.LogInformation($"✅ PDF aus Datei geladen ({pdfBytes.Length} bytes)");
                return pdfBytes;
            }

            // Fallback: In-Memory PDF
            if (_pdfInMemory != null && _pdfInMemory.Length > 0)
            {
                _logger.LogInformation($"✅ PDF aus Memory geladen ({_pdfInMemory.Length} bytes)");
                return _pdfInMemory;
            }

            // Fallback: Aus Konfiguration (für Development)
            var pdfBase64 = _configuration["Pdf:ContentBase64"];
            if (!string.IsNullOrEmpty(pdfBase64))
            {
                byte[] pdfBytes = Convert.FromBase64String(pdfBase64);
                _logger.LogInformation($"✅ PDF aus Config geladen ({pdfBytes.Length} bytes)");
                return pdfBytes;
            }

            _logger.LogWarning("⚠️ Keine PDF vorhanden");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Fehler beim Laden der PDF: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Speichert eine neue PDF (In-Memory oder Datei)
    /// </summary>
    public bool SavePdfContent(byte[] pdfBytes)
    {
        try
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                _logger.LogWarning("❌ Versuch, leere PDF zu speichern");
                return false;
            }

            // Versuche in Datei zu speichern
            try
            {
                // Always keep exactly one uploaded PDF.
                if (File.Exists(_pdfStoragePath))
                {
                    File.Delete(_pdfStoragePath);
                }

                File.WriteAllBytes(_pdfStoragePath, pdfBytes);
                _logger.LogInformation($"✅ PDF in Datei gespeichert ({pdfBytes.Length} bytes)");
            }
            catch
            {
                // Fallback: In Memory speichern
                _pdfInMemory = pdfBytes;
                _logger.LogWarning($"⚠️ PDF im Memory gespeichert (Datei-Fehler) ({pdfBytes.Length} bytes)");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Fehler beim Speichern der PDF: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Löscht die aktuelle PDF
    /// </summary>
    public bool DeletePdfContent()
    {
        try
        {
            // Lösche Datei
            if (File.Exists(_pdfStoragePath))
            {
                File.Delete(_pdfStoragePath);
                _logger.LogInformation("✅ PDF-Datei gelöscht");
            }

            // Lösche Memory
            _pdfInMemory = null;
            _logger.LogInformation("✅ PDF gelöscht");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Fehler beim Löschen der PDF: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Prüft ob User authentifiziert ist
    /// </summary>
    public bool IsValidRequest(HttpContext context)
    {
        var isAuthenticated = context.Session.GetString("IsAuthenticated") == "true";
        
        if (!isAuthenticated)
        {
            _logger.LogWarning($"❌ Unauthentifizierter Zugriff von {context.Connection.RemoteIpAddress}");
        }

        return isAuthenticated;
    }

    /// <summary>
    /// Prüft ob Admin authentifiziert ist
    /// </summary>
    public bool IsAdminRequest(HttpContext context)
    {
        var isAdmin = context.Session.GetString("IsAdmin") == "true";
        
        if (!isAdmin)
        {
            _logger.LogWarning($"❌ Unautorisierter Admin-Zugriff von {context.Connection.RemoteIpAddress}");
        }

        return isAdmin;
    }
}
