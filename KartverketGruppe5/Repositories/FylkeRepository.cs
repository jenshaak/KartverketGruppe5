using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using MySqlConnector;
using Dapper;

namespace KartverketGruppe5.Repositories
{
    /// <summary>
    /// Repository for fylker
    /// </summary>
    public class FylkeRepository : IFylkeRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<FylkeRepository> _logger;

        // SQL-spørringer som konstanter for bedre vedlikehold
        private const string SELECT_FYLKE_BASE = @"
            SELECT 
                fylkeId,
                navn 
            FROM Fylke";

        public FylkeRepository(
            IConfiguration configuration, 
            ILogger<FylkeRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Henter alle fylker, sortert alfabetisk
        /// </summary>
        public async Task<List<Fylke>> GetAllFylker()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var sql = $"{SELECT_FYLKE_BASE} ORDER BY navn";
                
                var fylker = await connection.QueryAsync<Fylke>(sql);
                _logger.LogDebug("Hentet {AntallFylker} fylker", fylker.Count());
                
                return fylker.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av fylker");
                throw;
            }
        }

        /// <summary>
        /// Henter et spesifikt fylke basert på ID
        /// </summary>
        public async Task<Fylke?> GetById(int fylkeId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var sql = $"{SELECT_FYLKE_BASE} WHERE fylkeId = @FylkeId";
                
                var fylke = await connection.QueryFirstOrDefaultAsync<Fylke>(sql, new { FylkeId = fylkeId });
                
                if (fylke == null)
                {
                    _logger.LogWarning("Fant ikke fylke med ID {FylkeId}", fylkeId);
                }
                
                return fylke;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av fylke {FylkeId}", fylkeId);
                throw;
            }
        }
    }
} 