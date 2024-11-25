using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using MySqlConnector;
using Dapper;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Repositories
{
    /// <summary>
    /// Repository for brukere
    /// </summary>
    public class BrukerRepository : IBrukerRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<BrukerRepository> _logger;

        // SQL-spørringer som konstanter for bedre vedlikehold
        private const string SELECT_BRUKER_BASE = @"
            SELECT 
                brukerId,
                fornavn,
                etternavn,
                email,
                passord,
                opprettetDato,
                slettet
            FROM Bruker";

        public BrukerRepository(
            IConfiguration configuration, 
            ILogger<BrukerRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Henter bruker basert på email, ignorerer slettede brukere
        /// </summary>
        public async Task<Bruker?> GetByEmail(string email)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var sql = $"{SELECT_BRUKER_BASE} WHERE email = @Email AND slettet = FALSE";
                
                var bruker = await connection.QueryFirstOrDefaultAsync<Bruker>(sql, new { Email = email });
                
                if (bruker == null)
                {
                    _logger.LogDebug("Ingen aktiv bruker funnet med email: {Email}", email);
                }
                
                return bruker;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av bruker med email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Oppretter ny bruker
        /// </summary>
        /// <returns>true hvis vellykket, false hvis ikke</returns>
        public async Task<bool> Create(Bruker bruker)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    INSERT INTO Bruker (
                        fornavn, 
                        etternavn, 
                        email, 
                        passord,
                        opprettetDato,
                        slettet
                    ) VALUES (
                        @Fornavn, 
                        @Etternavn, 
                        @Email, 
                        @Passord,
                        @OpprettetDato,
                        FALSE
                    )";

                var parameters = new
                {
                    bruker.Fornavn,
                    bruker.Etternavn,
                    bruker.Email,
                    bruker.Passord,
                    OpprettetDato = DateTime.Now
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                
                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Opprettet ny bruker: {Email}", bruker.Email);
                    return true;
                }
                
                _logger.LogWarning("Kunne ikke opprette bruker: {Email}", bruker.Email);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved opprettelse av bruker: {Email}", bruker.Email);
                throw;
            }
        }

        /// <summary>
        /// Oppdaterer eksisterende bruker
        /// </summary>
        /// <returns>true hvis vellykket, false hvis bruker ikke finnes</returns>
        public async Task<bool> Update(Bruker bruker)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    UPDATE Bruker 
                    SET 
                        fornavn = @Fornavn,
                        etternavn = @Etternavn,
                        email = @Email
                    WHERE brukerId = @BrukerId 
                    AND slettet = FALSE";

                var rowsAffected = await connection.ExecuteAsync(sql, bruker);
                
                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Oppdatert bruker: {Email}", bruker.Email);
                    return true;
                }
                
                _logger.LogWarning("Bruker ikke funnet: {Email}", bruker.Email);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved oppdatering av bruker: {Email}", bruker.Email);
                throw;
            }
        }

        public async Task<bool> SoftDelete(int brukerId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var affected = await connection.ExecuteAsync(@"
                    UPDATE Bruker 
                    SET Slettet = TRUE 
                    WHERE brukerId = @BrukerId",
                    new { BrukerId = brukerId });
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved soft delete av bruker: {BrukerId}", brukerId);
                throw new Exception("Kunne ikke deaktivere bruker", ex);
            }
        }
    }
} 