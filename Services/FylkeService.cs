using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Dapper;
using MySqlConnector;
using System.Data;
using KartverketGruppe5.Models;
namespace KartverketGruppe5.Services
{
    public class FylkeService
    {
        private readonly string _connectionString;
        private readonly ILogger<FylkeService> _logger;

        public FylkeService(IConfiguration configuration, ILogger<FylkeService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<List<Fylke>> GetAllFylker()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    SELECT 
                        fylkeId,
                        navn 
                    FROM Fylke 
                    ORDER BY navn";

                var fylker = await connection.QueryAsync<Fylke>(sql);

                var fylkeListe = fylker.ToList();
                _logger.LogInformation("Hentet {AntallFylker} fylker", fylkeListe.Count);

                return fylkeListe;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error ved henting av fylker");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil ved henting av fylker");
                throw;
            }
        }
    }
}