using AspNetCoreRateLimit;
using CVWebsite.Services;
using CVWebsite.Middleware;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CVWebsite.Csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (string.IsNullOrWhiteSpace(dataProtectionKeysPath) && !builder.Environment.IsDevelopment())
{
    dataProtectionKeysPath = "/data/dp-keys";
}

if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

// 🔧 FIX: Required for Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = "CVWebsite.Session";
});

// 🔧 FIX: Required for Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule { Endpoint = "*", Limit = 100, Period = "1m" },
        new RateLimitRule { Endpoint = "*/api/auth/login-auto", Limit = 5, Period = "1m" },
        new RateLimitRule { Endpoint = "*/api/auth/login", Limit = 5, Period = "1m" },
        new RateLimitRule { Endpoint = "*/api/auth/admin-login", Limit = 5, Period = "1m" }
    };
});

var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(",") ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddSingleton<ILoginAttemptService, LoginAttemptService>();

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors("AllowedOrigins");

// Reihenfolge ist wichtig
app.UseIpRateLimiting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api/auth/csrf", (IAntiforgery antiforgery, HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new { token = tokens.RequestToken });
});
app.MapGet("/health", () => Results.Ok("Healthy"));
app.MapGet("/", () => Results.Ok(new
{
    service = "CV Website API",
    status = "running",
    health = "/health",
    api = "/api",
    auth = new
    {
        csrf = "/api/auth/csrf",
        loginAuto = "/api/auth/login-auto"
    }
}));

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
