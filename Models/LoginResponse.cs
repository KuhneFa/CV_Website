namespace CVWebsite.Models;

/// <summary>
/// Response nach erfolgreichem Login
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
