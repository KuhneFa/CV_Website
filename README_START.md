# CV Website - Startguide

## Was wurde erstellt? ✅

**Komplette Skeleton-Struktur mit:**
- ✅ Controllers (Auth + PDF)
- ✅ Services (Authentifizierung + PDF-Handling)  
- ✅ Models (Login-Request/Response)
- ✅ Middleware (Security Headers)
- ✅ Program.cs (vollständige Konfiguration)
- ✅ Dockerfile + docker-compose
- ✅ Rate Limiting (gegen Bots)
- ✅ Session Management

## Nächste Schritte

### 1️⃣ **Passwort generieren (BCrypt Hash)**
```bash
cd /Users/fabiankassner/Documents/4\ Semester/Website/cv_website

# Terminal benutzen um BCrypt Hash zu erstellen:
dotnet run --project . --command "generate-hash"
# ODER manuell:
# echo "dein-passwort" | dotnet run
```

Dann den Hash in `appsettings.json` eintragen.

### 2️⃣ **PDF vorbereiten**
PDF in Base64 konvertieren:
```bash
base64 < LebenslaufOriginal.pdf > pdf_base64.txt
# Dann den Inhalt von pdf_base64.txt in appsettings.json → Pdf.ContentBase64 eintragen
```

### 3️⃣ **Lokal testen**
```bash
# Backend starten
dotnet run

# Im anderen Terminal: Test-API Call
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"password":"dein-passwort"}'
```

### 4️⃣ **Docker testen** (lokal)
```bash
docker-compose up
```

### 5️⃣ **GitHub pushen** (PDF bleibt lokal)
```bash
git add -A
git commit -m "Initial skeleton with auth & PDF handling"
git push
```

### 6️⃣ **Railway deployen**
- Railway Account erstellen
- Secrets eintragen (PasswordHash, PDF Base64)
- Dockerfile deployen

---

## Sicherheitsmerkale ✅

- ✅ BCrypt Password Hashing (timing-attack resistant)
- ✅ Session-basierte Auth (CSRF protected)
- ✅ Rate Limiting (5 Versuche/Min auf Login)
- ✅ Security Headers (CSP, HSTS, etc.)
- ✅ PDF nur für authentifizierte User
- ✅ Secrets nie im Repo

## Code Erklärung

### Services
- **AuthService**: Passwort-Validierung mit BCrypt
- **PdfService**: PDF-Laden aus Base64 + Authentifizierung prüfen

### Controller
- **AuthController**: `/api/auth/login`, `/api/auth/logout`, `/api/auth/status`
- **PdfController**: `/api/pdf/download` (nur auth)

### Middleware
- **SecurityHeadersMiddleware**: Setzt HTTPS, CSP, etc. Header

### Program.cs Highlights
```csharp
// Rate Limiting
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule { Endpoint = "*/api/auth/login", Limit = 5, Period = "1m" }
    };
});

// Dependency Injection
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPdfService, PdfService>();
```

Bereit zum Implementieren? Gib Bescheid! 🚀
