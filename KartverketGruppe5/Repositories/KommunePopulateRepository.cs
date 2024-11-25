using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using KartverketGruppe5.Repositories.Interfaces;

namespace KartverketGruppe5.Repositories
{
    /// <summary>
    /// Repository for kommuner
    /// </summary>
    public class KommunePopulateRepository : IKommunePopulateRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<KommunePopulateRepository> _logger;

        public KommunePopulateRepository(IConfiguration configuration, ILogger<KommunePopulateRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<IDbConnection> CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<List<string>> GetExistingKommuneNumbers()
        {
            using var db = await CreateConnection();
            return (await db.QueryAsync<string>("SELECT kommuneNummer FROM Kommune ORDER BY kommuneNummer")).ToList();
        }

        public async Task<List<string>> GetExistingFylkeNumbers()
        {
            using var db = await CreateConnection();
            return (await db.QueryAsync<string>("SELECT fylkeNummer FROM Fylke ORDER BY fylkeNummer")).ToList();
        }

        public async Task<bool> CheckFylkeExists(string fylkeNumber, string navn, IDbTransaction transaction)
        {
            const string sql = @"
                SELECT COUNT(*) FROM Fylke 
                WHERE fylkeNummer = @FylkeNummer 
                AND navn = @Navn";

            var count = await transaction.Connection.ExecuteScalarAsync<int>(
                sql, 
                new { FylkeNummer = fylkeNumber, Navn = navn.Trim() }, 
                transaction);

            return count > 0;
        }

        public async Task<int> InsertOrUpdateFylke(string fylkeNumber, string navn, IDbTransaction transaction)
        {
            const string sql = @"
                INSERT INTO Fylke (fylkeNummer, navn) 
                VALUES (@FylkeNummer, @Navn)
                ON DUPLICATE KEY UPDATE 
                    navn = @Navn,
                    fylkeId = LAST_INSERT_ID(fylkeId);
                SELECT LAST_INSERT_ID();";

            var fylkeId = await transaction.Connection.ExecuteScalarAsync<int>(
                sql,
                new { FylkeNummer = fylkeNumber, Navn = navn.Trim() },
                transaction);

            if (fylkeId == 0)
            {
                const string getFylkeId = "SELECT fylkeId FROM Fylke WHERE fylkeNummer = @FylkeNummer";
                fylkeId = await transaction.Connection.ExecuteScalarAsync<int>(
                    getFylkeId, 
                    new { FylkeNummer = fylkeNumber }, 
                    transaction);
            }

            return fylkeId;
        }

        public async Task<bool> CheckKommuneExists(string kommuneNumber, string navn, int fylkeId, IDbTransaction transaction)
        {
            const string sql = @"
                SELECT COUNT(*) FROM Kommune 
                WHERE kommuneNummer = @KommuneNummer 
                AND navn = @Navn 
                AND fylkeId = @FylkeId";

            var count = await transaction.Connection.ExecuteScalarAsync<int>(
                sql,
                new { KommuneNummer = kommuneNumber, Navn = navn.Trim(), FylkeId = fylkeId },
                transaction);

            return count > 0;
        }

        public async Task InsertOrUpdateKommune(string kommuneNumber, int fylkeId, string navn, IDbTransaction transaction)
        {
            const string sql = @"
                INSERT INTO Kommune (kommuneNummer, fylkeId, navn) 
                VALUES (@KommuneNummer, @FylkeId, @Navn)
                ON DUPLICATE KEY UPDATE 
                    fylkeId = @FylkeId,
                    navn = @Navn;";

            await transaction.Connection.ExecuteAsync(
                sql,
                new { KommuneNummer = kommuneNumber, FylkeId = fylkeId, Navn = navn.Trim() },
                transaction);
        }
    }
} 