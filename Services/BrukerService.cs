using KartverketGruppe5.Models;
using MySqlConnector;
using Dapper;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Services
{
    public class BrukerService : IBrukerService
    {
        private readonly MySqlConnection _connection;
        private readonly ILogger<BrukerService> _logger;

        public BrukerService(IConfiguration configuration, ILogger<BrukerService> logger)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _connection = new MySqlConnection(connectionString);
            _logger = logger;
        }

        public async Task<Bruker?> GetBrukerByEmail(string email)
        {
            try
            {
                return await _connection.QueryFirstOrDefaultAsync<Bruker>(
                    "SELECT * FROM Bruker WHERE email = @Email", 
                    new { Email = email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av bruker med email: {Email}", email);
                return null;
            }
        }

        public async Task<bool> CreateBruker(Bruker bruker)
        {
            // Sjekk om emailen allerede er i bruk
            if (await GetBrukerByEmail(bruker.Email) != null)
            {
                _logger.LogWarning("Forsøk på å opprette bruker med eksisterende email: {Email}", bruker.Email);
                return false;
            }

            bruker.Passord = HashPassword(bruker.Passord);
            
            try
            {
                await _connection.ExecuteAsync(@"
                    INSERT INTO Bruker (fornavn, etternavn, email, passord) 
                    VALUES (@Fornavn, @Etternavn, @Email, @Passord)",
                    new { 
                        bruker.Fornavn, 
                        bruker.Etternavn, 
                        bruker.Email, 
                        bruker.Passord 
                    });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved opprettelse av bruker: {Email}", bruker.Email);
                return false;
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        public async Task<IEnumerable<Bruker>> GetAlleBrukere()
        {
            try
            {
                return await _connection.QueryAsync<Bruker>(@"
                    SELECT brukerId, fornavn, etternavn, email, rolle, opprettetDato 
                    FROM Bruker 
                    ORDER BY opprettetDato DESC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av alle brukere");
                return Enumerable.Empty<Bruker>();
            }
        }

        public async Task<bool> OppdaterBruker(Bruker bruker)
        {
            try
            {
                var sql = @"
                    UPDATE Bruker
                    SET fornavn = @Fornavn, 
                        etternavn = @Etternavn, 
                        email = @Email
                    WHERE brukerId = @BrukerId";

                var affected = await _connection.ExecuteAsync(sql, 
                    new { 
                        bruker.Fornavn, 
                        bruker.Etternavn, 
                        bruker.Email, 
                        bruker.BrukerId 
                    });

                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av bruker");
                return false;
            }
        }
    }
} 