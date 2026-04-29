using BCrypt.Net;

namespace CVWebsite.Services;

public interface IAuthService
{
    bool ValidatePassword(string inputPassword);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool ValidatePassword(string inputPassword)
    {
        try
        {
            var storedHash = _configuration["Auth:PasswordHash"];
            
            if (string.IsNullOrEmpty(storedHash))
            {
                _logger.LogError("PasswordHash nicht konfiguriert!");
                return false;
            }

            if (string.IsNullOrEmpty(inputPassword))
            {
                _logger.LogWarning("Leeres Passwort versucht");
                return false;
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
            
            if (!isValid)
            {
                _logger.LogWarning("Ungültiges Passwort versucht");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fehler bei Passwort-Validierung: {ex.Message}");
            return false;
        }
    }
}
