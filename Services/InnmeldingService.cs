using Dapper;
using MySqlConnector;
using System.Data;
using KartverketGruppe5.Models;
namespace KartverketGruppe5.Services
{
    public class InnmeldingService
    {
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;
        private readonly ILogger<InnmeldingService> _logger;
        private readonly BildeService _bildeService;

        public InnmeldingService(IConfiguration configuration, ILogger<InnmeldingService> logger, BildeService bildeService)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            _bildeService = bildeService;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        public int AddInnmelding(int brukerId, int kommuneId, int lokasjonId, string beskrivelse, string? bildeSti)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = @"INSERT INTO Innmelding 
                               (brukerId, kommuneId, lokasjonId, beskrivelse, status, bildeSti) 
                               VALUES 
                               (@BrukerId, @KommuneId, @LokasjonId, @Beskrivelse, 'Ny', @BildeSti);
                               SELECT LAST_INSERT_ID();";

                return dbConnection.ExecuteScalar<int>(query, new
                {
                    BrukerId = brukerId,
                    KommuneId = kommuneId,
                    LokasjonId = lokasjonId,
                    Beskrivelse = beskrivelse,
                    BildeSti = bildeSti
                });
            }
        }

        public async Task<PagedResult<InnmeldingModel>> GetInnmeldinger(
            int? saksbehandlerId = null,
            int? innmelderBrukerId = null,
            string sortOrder = "date_desc",
            string statusFilter = "",
            string fylkeFilter = "",
            int page = 1,
            int pageSize = 10)
        {
            using var connection = new MySqlConnection(_connectionString);
            
            var sql = @"
                SELECT 
                    i.innmeldingId,
                    i.brukerId,
                    i.kommuneId,
                    i.lokasjonId,
                    i.beskrivelse,
                    i.status,
                    i.opprettetDato,
                    i.saksbehandlerId,
                    k.navn as kommuneNavn";
                
            if (!saksbehandlerId.HasValue && !innmelderBrukerId.HasValue)
            {
                sql += @",
                    f.navn as fylkeNavn";
            }

            sql += @" FROM Innmelding i
                      INNER JOIN Kommune k ON i.kommuneId = k.kommuneId";

            if (!saksbehandlerId.HasValue && !innmelderBrukerId.HasValue)
            {
                sql += " INNER JOIN Fylke f ON k.fylkeId = f.fylkeId";
            }
            _logger.LogInformation($"sql: {sql}");

            // Build WHERE clause
            var whereConditions = new List<string>();
            if (saksbehandlerId.HasValue)
            {
                whereConditions.Add("i.saksbehandlerId = @SaksbehandlerId");
            }
            if (innmelderBrukerId.HasValue)
            {
                whereConditions.Add("i.brukerId = @InnmelderBrukerId");
            }
            if (!string.IsNullOrEmpty(statusFilter))
            {
                whereConditions.Add("i.status = @Status");
            }
            if (!string.IsNullOrEmpty(fylkeFilter))
            {
                whereConditions.Add("f.navn = @FylkeFilter");
            }

            if (whereConditions.Any())
            {
                sql += " WHERE " + string.Join(" AND ", whereConditions);
            }

            // Count total before pagination
            var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
            var totalItems = await connection.ExecuteScalarAsync<int>(countSql, new { 
                SaksbehandlerId = saksbehandlerId,
                InnmelderBrukerId = innmelderBrukerId,
                Status = statusFilter,
                FylkeFilter = fylkeFilter
            });

            // Add sorting
            sql += sortOrder switch
            {
                "date_asc" => " ORDER BY i.opprettetDato ASC",
                _ => " ORDER BY i.opprettetDato DESC"
            };

            // Add pagination
            sql += " LIMIT @Skip, @Take";

            var innmeldinger = await connection.QueryAsync<InnmeldingModel>(sql, new { 
                SaksbehandlerId = saksbehandlerId,
                InnmelderBrukerId = innmelderBrukerId,
                Status = statusFilter,
                FylkeFilter = fylkeFilter,
                Skip = (page - 1) * pageSize,
                Take = pageSize
            });

            // Debug: Sjekk første innmelding
            var firstItem = innmeldinger.FirstOrDefault();
            if (firstItem != null)
            {
                _logger.LogInformation($"Første innmelding: Kommune={firstItem.KommuneNavn}, Fylke={firstItem.FylkeNavn}");
            }

            var items = innmeldinger.Select(i => new InnmeldingModel
            {
                InnmeldingId = i.InnmeldingId,
                BrukerId = i.BrukerId,
                KommuneId = i.KommuneId,
                LokasjonId = i.LokasjonId,
                Beskrivelse = i.Beskrivelse,
                Status = i.Status,
                OpprettetDato = i.OpprettetDato,
                KommuneNavn = i.KommuneNavn,
                FylkeNavn = i.FylkeNavn,
                SaksbehandlerId = i.SaksbehandlerId,
                StatusClass = GetStatusClass(i.Status)
            }).ToList();

            return new PagedResult<InnmeldingModel>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<InnmeldingModel> GetInnmeldingById(int id, bool includeKommuneNavn = false, bool includeLokasjon = false)
        {
            using var connection = new MySqlConnection(_connectionString);
            
            string sql = @"
                SELECT 
                    i.innmeldingId,
                    i.brukerId,
                    i.kommuneId,
                    i.lokasjonId,
                    i.beskrivelse,
                    i.status,
                    i.opprettetDato,
                    i.saksbehandlerId,
                    i.bildeSti";

            if (includeKommuneNavn)
            {
                sql += ", k.navn as kommuneNavn";
            }

            if (includeLokasjon)
            {
                sql += ", l.latitude, l.longitude, l.geoJson";
            }

            sql += " FROM Innmelding i";

            if (includeKommuneNavn)
            {
                sql += " INNER JOIN Kommune k ON i.kommuneId = k.kommuneId";
            }

            if (includeLokasjon)
            {
                sql += " INNER JOIN Lokasjon l ON i.lokasjonId = l.lokasjonId";
            }

            sql += " WHERE i.innmeldingId = @Id";

            var innmelding = await connection.QueryFirstOrDefaultAsync<InnmeldingModel>(sql, new { Id = id });

            if (innmelding != null)
            {
                innmelding.StatusClass = GetStatusClass(innmelding.Status);
            }

            return innmelding;
        }

        public async Task UpdateInnmeldingDetails(InnmeldingModel innmelding, LokasjonModel? lokasjon = null, IFormFile? bilde = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            var oppdatertDato = DateTime.Now;

            if (bilde != null)
            {
                innmelding.BildeSti = await _bildeService.LagreBilde(bilde, innmelding.InnmeldingId);
            }

            if (lokasjon != null)
            {
                await UpdateLokasjon(lokasjon, oppdatertDato);
            }

            string query = @"
                UPDATE Innmelding SET 
                    beskrivelse = @Beskrivelse,
                    bildeSti = COALESCE(@BildeSti, bildeSti),
                    oppdatertDato = @OppdatertDato
                WHERE innmeldingId = @InnmeldingId";

            await connection.ExecuteAsync(query, new { 
                innmelding.Beskrivelse, 
                innmelding.BildeSti,
                OppdatertDato = oppdatertDato, 
                innmelding.InnmeldingId 
            });
        }

        public async Task UpdateInnmeldingStatus(int innmeldingId, string status, int? saksbehandlerId = null, string? kommentar = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            var oppdatertDato = DateTime.Now;

            var updateFields = new List<string> { "status = @Status", "oppdatertDato = @OppdatertDato" };
            var parameters = new DynamicParameters();
            parameters.Add("Status", status);
            parameters.Add("OppdatertDato", oppdatertDato);
            parameters.Add("InnmeldingId", innmeldingId);

            if (saksbehandlerId.HasValue)
            {
                updateFields.Add("saksbehandlerId = @SaksbehandlerId");
                parameters.Add("SaksbehandlerId", saksbehandlerId.Value);
            }

            if (!string.IsNullOrEmpty(kommentar))
            {
                updateFields.Add("kommentar = @Kommentar");
                parameters.Add("Kommentar", kommentar);
            }

            string query = $@"
                UPDATE Innmelding 
                SET {string.Join(", ", updateFields)}
                WHERE innmeldingId = @InnmeldingId";

            await connection.ExecuteAsync(query, parameters);
        }

        private async Task UpdateLokasjon(LokasjonModel lokasjon, DateTime oppdatertDato)
        {
            using var connection = new MySqlConnection(_connectionString);
            string updateLokasjonQuery = @"
                UPDATE Lokasjon SET 
                    geoJson = @GeoJson,
                    geometriType = @GeometriType,
                    latitude = @Latitude,
                    longitude = @Longitude,
                    oppdatertDato = @OppdatertDato
                WHERE lokasjonId = @LokasjonId";

            await connection.ExecuteAsync(updateLokasjonQuery, new { 
                lokasjon.GeoJson, 
                lokasjon.GeometriType, 
                lokasjon.Latitude, 
                lokasjon.Longitude, 
                OppdatertDato = oppdatertDato, 
                lokasjon.LokasjonId 
            });
        }

        public void UpdateStatus(int innmeldingId, string status)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = "UPDATE Innmelding SET status = @Status WHERE innmeldingId = @Id";
                dbConnection.Execute(query, new { Id = innmeldingId, Status = status });
            }
        }

        public void AddKommentar(int innmeldingId, string kommentar)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = "UPDATE Innmelding SET kommentar = @Kommentar WHERE innmeldingId = @Id";
                dbConnection.Execute(query, new { Id = innmeldingId, Kommentar = kommentar });
            }
        }

        public InnmeldingModel GetInnmeldingForLokasjon(int lokasjonId)
        {
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    const string sql = @"
                        SELECT 
                            innmeldingId,
                            brukerId,
                            kommuneId,
                            lokasjonId,
                            beskrivelse,
                            opprettetDato
                        FROM Innmelding 
                        WHERE lokasjonId = @LokasjonId
                        ORDER BY opprettetDato DESC
                        LIMIT 1";

                    return db.QueryFirstOrDefault<InnmeldingModel>(sql, new { LokasjonId = lokasjonId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting innmelding for lokasjon {lokasjonId}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateInnmelding(Innmelding innmelding)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    UPDATE Innmelding 
                    SET status = @Status,
                        saksbehandlerId = @SaksbehandlerId,
                        oppdatertDato = @OppdatertDato
                    WHERE innmeldingId = @InnmeldingId";
                
                innmelding.OppdatertDato = DateTime.Now;
                
                var rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    innmelding.Status,
                    innmelding.SaksbehandlerId,
                    innmelding.OppdatertDato,
                    innmelding.InnmeldingId
                });
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating innmelding: {ex.Message}");
                return false;
            }
        }

        public async Task UpdateBildeSti(int innmeldingId, string bildeSti)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = "UPDATE Innmelding SET bildeSti = @BildeSti WHERE innmeldingId = @InnmeldingId";
                await dbConnection.ExecuteAsync(query, new { InnmeldingId = innmeldingId, BildeSti = bildeSti });
            }
        }

        private string GetStatusClass(string status) => status switch
        {
            "Ny" => "bg-blue-100 text-blue-800",
            "Under behandling" => "bg-yellow-100 text-yellow-800",
            "Godkjent" => "bg-green-100 text-green-800",
            "Avvist" => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };

        public static List<string> GetAllStatuses()
        {
            return new List<string> 
            { 
                "Ny",
                "Under behandling",
                "Godkjent",
                "Avvist"
            };
        }
    }
} 