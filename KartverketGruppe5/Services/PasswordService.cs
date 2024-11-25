using System.Security.Cryptography;
using System.Text;
using KartverketGruppe5.Services.Interfaces;

namespace KartverketGruppe5.Services;

public class PasswordService : IPasswordService
{
    /// <summary>
    /// Hasher en passord
    /// </summary>
    /// <param name="password">Passordet som skal hashes</param>
    /// <returns>Hashet passord</returns>
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Sjekker om et passord er riktig
    /// </summary>
    /// <param name="password">Passordet som skal sjekkes</param>
    /// <param name="hashedPassword">Hashet passord</param>
    /// <returns>True hvis passordet er riktig, False hvis ikke</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
} 