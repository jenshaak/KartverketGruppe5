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
            _logger.LogInformation("getting fylker");
            using (IDbConnection db = new MySqlConnection(_connectionString))
            {
                const string sql = @"SELECT * FROM Fylke";
                var fylker = await db.QueryAsync<Fylke>(sql);
                return fylker.ToList();
            }
        }
    }
}