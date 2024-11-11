using System.Data;
using System.Text.Json;
using KartverketGruppe5.API_Models;
using Dapper;
using MySqlConnector;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

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

        private string GetFylkesnavnFromKommunenummer(string kommunenummer)
        {
            // Første to siffer i kommunenummeret indikerer fylket
            if (kommunenummer.Length >= 2)
            {
                string fylkesnummer = kommunenummer.Substring(0, 2);
                return fylkesnummer switch
                {
                    "03" => "Oslo",
                    "11" => "Rogaland",
                    "15" => "Møre og Romsdal",
                    "18" => "Nordland",
                    "31" => "Østfold",
                    "32" => "Akershus",
                    "33" => "Buskerud",
                    "34" => "Innlandet",
                    "39" => "Vestfold",
                    "40" => "Telemark",
                    "42" => "Agder",
                    "46" => "Vestland",
                    "50" => "Trøndelag",
                    "55" => "Troms",
                    "56" => "Finnmark",
                    _ => throw new ArgumentException($"Ukjent fylkesnummer: {fylkesnummer} for kommune {kommunenummer}")
                };
            }
            throw new ArgumentException($"Invalid kommunenummer: {kommunenummer}");
        }

        public async Task<string> PopulateFylkerOgKommuner()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiSettings.KommuneInfoApiBaseUrl}/kommuner");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                
                var alleKommuner = JsonSerializer.Deserialize<List<KommuneInfo>>(json);
                _logger.LogInformation($"Hentet {alleKommuner?.Count ?? 0} kommuner fra API");

                var kommuneNumreFraApi = alleKommuner.Select(k => k.Kommunenummer).OrderBy(n => n).ToList();

                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    db.Open();

                    var eksisterendeKommuner = db.Query<string>("SELECT kommuneNummer FROM Kommune ORDER BY kommuneNummer").ToList();
                    _logger.LogInformation($"Antall kommuner i database: {eksisterendeKommuner.Count}");
                    var eksisterendeFylker = db.Query<string>("SELECT fylkeNummer FROM Fylke ORDER BY fylkeNummer").ToList();
                    _logger.LogInformation($"Antall fylker i database: {eksisterendeFylker.Count}");

                    var mangler = kommuneNumreFraApi.Except(eksisterendeKommuner).ToList();
                    var ekstra = eksisterendeKommuner.Except(kommuneNumreFraApi).ToList();

                    if (mangler.Any())
                    {
                        var manglendeKommuner = alleKommuner
                            .Where(k => mangler.Contains(k.Kommunenummer))
                            .Select(k => new {
                                Nummer = k.Kommunenummer,
                                Navn = k.KommunenavnNorsk,
                                FylkeNummer = k.Kommunenummer.Substring(0, 2)
                            })
                            .ToList();

                        foreach (var kommune in manglendeKommuner)
                        {
                            _logger.LogWarning(
                                $"Kommune mangler: {kommune.Navn} ({kommune.Nummer}) i fylke {GetFylkesnavnFromKommunenummer(kommune.FylkeNummer)}");
                        }
                    }

                    if (ekstra.Any())
                    {
                        _logger.LogWarning($"Fant {ekstra.Count} kommuner i databasen som ikke finnes i API-et");
                        var ekstraKommuner = db.Query<string>(
                            "SELECT CONCAT(navn, ' (', kommuneNummer, ')') FROM Kommune WHERE kommuneNummer IN @Numre", 
                            new { Numre = ekstra }).ToList();
                        _logger.LogWarning($"Overflødige kommuner: {string.Join(", ", ekstraKommuner)}");
                    }

                    using (var transaction = db.BeginTransaction())
                    {
                        try
                        {
                            bool harEndringer = false;
                            int oppdaterteFylker = 0;
                            int oppdaterteKommuner = 0;

                            var fylkesGrupper = alleKommuner
                                .Select(k => new {
                                    Kommune = k,
                                    FylkeNummer = k.Kommunenummer.Substring(0, 2),
                                    Fylkesnavn = GetFylkesnavnFromKommunenummer(k.Kommunenummer)
                                })
                                .GroupBy(x => x.FylkeNummer)
                                .ToList();

                            foreach (var fylkeGruppe in fylkesGrupper)
                            {
                                string fylkesnavn = GetFylkesnavnFromKommunenummer(fylkeGruppe.Key + "00");

                                // Sjekk om fylket er endret
                                string checkFylke = @"
                                    SELECT COUNT(*) FROM Fylke 
                                    WHERE fylkeNummer = @FylkeNummer 
                                    AND navn = @Navn";

                                var fylkeEksisterer = db.ExecuteScalar<int>(checkFylke, new { 
                                    FylkeNummer = fylkeGruppe.Key,
                                    Navn = fylkesnavn.Trim() 
                                }, transaction) > 0;

                                string insertFylke = @"
                                    INSERT INTO Fylke (fylkeNummer, navn) 
                                    VALUES (@FylkeNummer, @Navn)
                                    ON DUPLICATE KEY UPDATE 
                                        navn = @Navn,
                                        fylkeId = LAST_INSERT_ID(fylkeId);
                                    SELECT LAST_INSERT_ID();";

                                var fylkeId = db.ExecuteScalar<int>(insertFylke, new { 
                                    FylkeNummer = fylkeGruppe.Key,
                                    Navn = fylkesnavn.Trim() 
                                }, transaction);

                                if (!fylkeEksisterer)
                                {
                                    oppdaterteFylker++;
                                    harEndringer = true;
                                }

                                if (fylkeId == 0)
                                {
                                    string getFylkeId = "SELECT fylkeId FROM Fylke WHERE fylkeNummer = @FylkeNummer";
                                    fylkeId = db.ExecuteScalar<int>(getFylkeId, new { FylkeNummer = fylkeGruppe.Key }, transaction);
                                }

                                foreach (var item in fylkeGruppe)
                                {
                                    var kommune = item.Kommune;

                                    // Sjekk om kommunen er endret
                                    string checkKommune = @"
                                        SELECT COUNT(*) FROM Kommune 
                                        WHERE kommuneNummer = @KommuneNummer 
                                        AND navn = @Navn 
                                        AND fylkeId = @FylkeId";

                                    var kommuneEksisterer = db.ExecuteScalar<int>(checkKommune, new { 
                                        KommuneNummer = kommune.Kommunenummer,
                                        Navn = kommune.KommunenavnNorsk.Trim(),
                                        FylkeId = fylkeId
                                    }, transaction) > 0;

                                    if (!kommuneEksisterer)
                                    {
                                        string insertKommune = @"
                                            INSERT INTO Kommune (kommuneNummer, fylkeId, navn) 
                                            VALUES (@KommuneNummer, @FylkeId, @Navn)
                                            ON DUPLICATE KEY UPDATE 
                                                fylkeId = @FylkeId,
                                                navn = @Navn;";

                                        db.Execute(insertKommune, new { 
                                            KommuneNummer = kommune.Kommunenummer,
                                            FylkeId = fylkeId,
                                            Navn = kommune.KommunenavnNorsk.Trim()
                                        }, transaction);

                                        oppdaterteKommuner++;
                                        harEndringer = true;
                                    }
                                }
                            }

                            transaction.Commit();

                            if (!harEndringer)
                            {
                                return $"Alle fylker og kommuner er allerede oppdatert til siste versjon. {eksisterendeKommuner.Count} kommuner og {eksisterendeFylker.Count} fylker totalt.";
                            }
                            
                            return $"Oppdatert {oppdaterteFylker} fylker og {oppdaterteKommuner} kommuner. Totalt {eksisterendeKommuner.Count} kommuner og {eksisterendeFylker.Count} fylker.";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.LogError($"Feil ved populering av database: {ex.Message}");
                            if (ex.InnerException != null)
                            {
                                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                            }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error populating database: {ex.Message}");
                throw;
            }
        }
    }
} 