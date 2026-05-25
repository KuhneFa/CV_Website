namespace CVWebsite.Middleware;

/// <summary>
/// Middleware für Sicherheits-Header
/// Schützt vor Common Attacks (XSS, Clickjacking, etc.)
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        ILogger<SecurityHeadersMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Sicherheits-Header setzen
        
        // HSTS: Erzwingt HTTPS für zukünftige Requests
        context.Response.Headers.Append(
            "Strict-Transport-Security", 
            "max-age=31536000; includeSubDomains"
        );

        var isPdfDownload = context.Request.Path.StartsWithSegments("/api/pdf/download");

        // CSP: Verhindert XSS durch Einschränkung von Inline-Scripts
        context.Response.Headers.Append(
            "Content-Security-Policy",
            isPdfDownload
                ? $"default-src 'none'; frame-ancestors 'self' {GetFrameAncestors()}"
                : "default-src 'self'; object-src 'none'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self'"
        );

        // X-Content-Type-Options: Verhindert MIME-Sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: Schutz vor Clickjacking. Für PDF-Preview nutzen wir CSP frame-ancestors,
        // weil X-Frame-Options keine kontrollierte Freigabe für andere Subdomains erlaubt.
        if (!isPdfDownload)
        {
            context.Response.Headers.Append("X-Frame-Options", "DENY");
        }

        // X-XSS-Protection: Zusätzlicher XSS-Schutz (legacy browser)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Referrer-Policy: Kontrolle von Referrer-Information
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy: Einschränkung gefährlicher Features
        context.Response.Headers.Append(
            "Permissions-Policy",
            "geolocation=(), microphone=(), camera=(), payment=()"
        );

        _logger.LogDebug($"Security Headers für {context.Request.Path} gesetzt");

        await _next(context);
    }

    private string GetFrameAncestors()
    {
        var configuredOrigins = _configuration["Cors:AllowedOrigins"]
            ?.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        var origins = configuredOrigins.Length > 0
            ? configuredOrigins
            : new[] { "http://localhost:3000" };

        return string.Join(" ", origins);
    }
}
