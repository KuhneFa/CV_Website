namespace CVWebsite.Models;

/// <summary>
/// Login-Request vom Frontend
/// </summary>
public class LoginRequest
{
    public string? Password { get; set; }
    public string? Website { get; set; }
}
