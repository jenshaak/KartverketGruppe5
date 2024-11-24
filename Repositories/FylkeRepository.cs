using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using MySqlConnector;
using Dapper;

namespace KartverketGruppe5.Repositories
{
    public class FylkeRepository : IFylkeRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<FylkeRepository> _logger;

        public FylkeRepository(IConfiguration configuration, ILogger<FylkeRepository> logger)
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
                return fylker.ToList();
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error ved henting av fylker");
                throw;
            }
        }

        public async Task<Fylke?> GetById(int fylkeId)
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
                    WHERE fylkeId = @FylkeId";

                return await connection.QueryFirstOrDefaultAsync<Fylke>(sql, new { FylkeId = fylkeId });
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error ved henting av fylke {FylkeId}", fylkeId);
                throw;
            }
        }
    }
} 