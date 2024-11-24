using System.Data;
using System.Text.Json;
using KartverketGruppe5.API_Models;
using Dapper;
using MySqlConnector;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Repositories.Interfaces;

namespace KartverketGruppe5.Services
{
    public class KommunePopulateService : IKommunePopulateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KommunePopulateService> _logger;
        private readonly ApiSettings _apiSettings;
        private readonly IKommunePopulateRepository _repository;

        public KommunePopulateService(
            HttpClient httpClient,
            ILogger<KommunePopulateService> logger,
            IOptions<ApiSettings> apiSettings,
            IKommunePopulateRepository repository)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiSettings = apiSettings.Value;
            _repository = repository;
        }

        public string GetFylkesnavnFromKommunenummer(string kommunenummer)
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
                if (alleKommuner == null)
                {
                    throw new InvalidOperationException("Kunne ikke deserialisere kommunedata fra API");
                }
                
                _logger.LogInformation($"Hentet {alleKommuner.Count} kommuner fra API");

                var kommuneNumreFraApi = alleKommuner.Select(k => k.Kommunenummer).OrderBy(n => n).ToList();

                using (var db = await _repository.CreateConnection())
                {
                    var eksisterendeKommuner = await _repository.GetExistingKommuneNumbers();
                    _logger.LogInformation($"Antall kommuner i database: {eksisterendeKommuner.Count}");
                    
                    var eksisterendeFylker = await _repository.GetExistingFylkeNumbers();
                    _logger.LogInformation($"Antall fylker i database: {eksisterendeFylker.Count}");

                    // Logg manglende og ekstra kommuner
                    LogKommuneDifferences(alleKommuner, kommuneNumreFraApi, eksisterendeKommuner, db);

                    using (var transaction = db.BeginTransaction())
                    {
                        try
                        {
                            var (harEndringer, oppdaterteFylker, oppdaterteKommuner) = 
                                await UpdateFylkerOgKommuner(alleKommuner, transaction);

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

        private async Task<(bool harEndringer, int oppdaterteFylker, int oppdaterteKommuner)> 
            UpdateFylkerOgKommuner(List<KommuneInfo> alleKommuner, IDbTransaction transaction)
        {
            bool harEndringer = false;
            int oppdaterteFylker = 0;
            int oppdaterteKommuner = 0;

            var fylkesGrupper = alleKommuner
                .Select(k => new {
                    Kommune = k,
                    FylkeNummer = k.Kommunenummer?.Substring(0, 2) 
                        ?? throw new ArgumentNullException(nameof(k.Kommunenummer)),
                    Fylkesnavn = GetFylkesnavnFromKommunenummer(k.Kommunenummer 
                        ?? throw new ArgumentNullException(nameof(k.Kommunenummer)))
                })
                .GroupBy(x => x.FylkeNummer)
                .ToList();

            foreach (var fylkeGruppe in fylkesGrupper)
            {
                string fylkesnavn = GetFylkesnavnFromKommunenummer(fylkeGruppe.Key + "00");

                var fylkeEksisterer = await _repository.CheckFylkeExists(fylkeGruppe.Key, fylkesnavn, transaction);
                var fylkeId = await _repository.InsertOrUpdateFylke(fylkeGruppe.Key, fylkesnavn, transaction);

                if (!fylkeEksisterer)
                {
                    oppdaterteFylker++;
                    harEndringer = true;
                }

                foreach (var item in fylkeGruppe)
                {
                    var kommune = item.Kommune;
                    if (kommune.KommunenavnNorsk == null)
                    {
                        throw new ArgumentNullException(nameof(kommune.KommunenavnNorsk));
                    }

                    var kommuneEksisterer = await _repository.CheckKommuneExists(
                        kommune.Kommunenummer ?? throw new ArgumentNullException(nameof(kommune.Kommunenummer)),
                        kommune.KommunenavnNorsk,
                        fylkeId,
                        transaction);

                    if (!kommuneEksisterer)
                    {
                        await _repository.InsertOrUpdateKommune(
                            kommune.Kommunenummer,
                            fylkeId,
                            kommune.KommunenavnNorsk,
                            transaction);

                        oppdaterteKommuner++;
                        harEndringer = true;
                    }
                }
            }

            return (harEndringer, oppdaterteFylker, oppdaterteKommuner);
        }

        private void LogKommuneDifferences(
            List<KommuneInfo> alleKommuner,
            List<string> kommuneNumreFraApi,
            List<string> eksisterendeKommuner,
            IDbConnection db)
        {
            var mangler = kommuneNumreFraApi.Except(eksisterendeKommuner).ToList();
            var ekstra = eksisterendeKommuner.Except(kommuneNumreFraApi).ToList();

            if (mangler.Any())
            {
                var manglendeKommuner = alleKommuner
                    .Where(k => mangler.Contains(k.Kommunenummer))
                    .Select(k => new {
                        Nummer = k.Kommunenummer,
                        Navn = k.KommunenavnNorsk,
                        FylkeNummer = k.Kommunenummer?.Substring(0, 2)
                    })
                    .ToList();

                foreach (var kommune in manglendeKommuner)
                {
                    _logger.LogWarning(
                        $"Kommune mangler: {kommune.Navn} ({kommune.Nummer}) i fylke {GetFylkesnavnFromKommunenummer(kommune.FylkeNummer ?? throw new ArgumentNullException(nameof(kommune.FylkeNummer)))}");
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
        }
    }
} 