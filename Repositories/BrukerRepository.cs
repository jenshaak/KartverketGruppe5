using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using MySqlConnector;
using Dapper;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Repositories
{
    public class BrukerRepository : IBrukerRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<BrukerRepository> _logger;

        public BrukerRepository(IConfiguration configuration, ILogger<BrukerRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<Bruker?> GetByEmail(string email)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<Bruker>(
                    "SELECT * FROM Bruker WHERE email = @Email AND Slettet = FALSE", 
                    new { Email = email });
            }
            catch (Exception ex)
            {
                throw new Exception("En uventet feil oppstod ved henting av bruker", ex);
            }
        }

        public async Task<bool> Create(Bruker bruker)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var affected = await connection.ExecuteAsync(@"
                INSERT INTO Bruker (fornavn, etternavn, email, passord) 
                VALUES (@Fornavn, @Etternavn, @Email, @Passord)",
                bruker);
                return affected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("En uventet feil oppstod ved opprettelse av bruker", ex);
            }
        }


        public async Task<bool> Update(Bruker bruker)
        {
            try 
            {
                using var connection = new MySqlConnection(_connectionString);
                var affected = await connection.ExecuteAsync(@"
                UPDATE Bruker
                SET fornavn = @Fornavn, 
                    etternavn = @Etternavn, 
                    email = @Email
                WHERE brukerId = @BrukerId",
                bruker);
                return affected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("En uventet feil oppstod ved oppdatering av bruker", ex);
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