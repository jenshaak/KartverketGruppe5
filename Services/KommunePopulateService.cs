using KartverketGruppe5.API_Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Data;
using MySqlConnector;
using Dapper;

namespace KartverketGruppe5.Services
{
    public class KommunePopulateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KommunePopulateService> _logger;
        private readonly ApiSettings _apiSettings;
        private readonly string _connectionString;

        public KommunePopulateService(
            HttpClient httpClient, 
            ILogger<KommunePopulateService> logger, 
            IOptions<ApiSettings> apiSettings,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiSettings = apiSettings.Value;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task PopulateFylkerOgKommuner()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiSettings.KommuneInfoApiBaseUrl}/kommuner");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Successfully fetched {json.Length} characters of data from API");
                
                var alleKommuner = JsonSerializer.Deserialize<List<KommuneInfo>>(json);
                _logger.LogInformation($"Deserialized {alleKommuner?.Count ?? 0} kommuner from API");

                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    db.Open();

                    using (var transaction = db.BeginTransaction())
                    {
                        try
                        {
                            var fylkesGrupper = alleKommuner
                                .Where(k => !string.IsNullOrEmpty(k.Fylkesnavn))
                                .GroupBy(k => k.Fylkesnummer)
                                .Select(g => new { 
                                    Fylkesnummer = g.Key, 
                                    Fylkesnavn = g.First().Fylkesnavn 
                                })
                                .ToList();

                            _logger.LogInformation($"Processing {fylkesGrupper.Count} fylker");

                            foreach (var fylke in fylkesGrupper)
                            {
                                _logger.LogInformation($"Processing fylke: {fylke.Fylkesnavn} (nummer: {fylke.Fylkesnummer})");

                                if (string.IsNullOrEmpty(fylke.Fylkesnavn))
                                {
                                    _logger.LogWarning($"Skipping fylke with null or empty name (nummer: {fylke.Fylkesnummer})");
                                    continue;
                                }

                                string insertFylke = @"
                                    INSERT INTO Fylke (navn) 
                                    VALUES (@Navn)
                                    ON DUPLICATE KEY UPDATE navn = @Navn;
                                    SELECT LAST_INSERT_ID();";

                                var fylkeId = db.ExecuteScalar<int>(insertFylke, new { 
                                    Navn = fylke.Fylkesnavn.Trim() 
                                }, transaction);

                                _logger.LogInformation($"Inserted/Updated fylke {fylke.Fylkesnavn} with ID {fylkeId}");

                                var kommuner = alleKommuner
                                    .Where(k => k.Fylkesnummer == fylke.Fylkesnummer && 
                                              !string.IsNullOrEmpty(k.KommunenavnNorsk))
                                    .ToList();

                                _logger.LogInformation($"Processing {kommuner.Count} kommuner for {fylke.Fylkesnavn}");

                                foreach (var kommune in kommuner)
                                {
                                    if (string.IsNullOrEmpty(kommune.KommunenavnNorsk))
                                    {
                                        _logger.LogWarning($"Skipping kommune with null or empty name in fylke {fylke.Fylkesnavn}");
                                        continue;
                                    }

                                    string insertKommune = @"
                                        INSERT INTO Kommune (fylkeId, navn) 
                                        VALUES (@FylkeId, @Navn)
                                        ON DUPLICATE KEY UPDATE navn = @Navn;";

                                    db.Execute(insertKommune, new { 
                                        FylkeId = fylkeId,
                                        Navn = kommune.KommunenavnNorsk.Trim()
                                    }, transaction);

                                    _logger.LogInformation($"Inserted/Updated kommune {kommune.KommunenavnNorsk}");
                                }
                            }

                            transaction.Commit();
                            _logger.LogInformation("Successfully committed all changes to database");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error during database operations: {ex.Message}");
                            _logger.LogError($"Stack trace: {ex.StackTrace}");
                            transaction.Rollback();
                            throw new Exception("Failed to populate database", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error populating database: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 