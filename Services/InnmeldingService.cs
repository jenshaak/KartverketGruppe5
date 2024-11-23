using Dapper;
using MySqlConnector;
using System.Data;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.RequestModels;
namespace KartverketGruppe5.Services
{
    public class InnmeldingService
    {
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;
        private readonly ILogger<InnmeldingService> _logger;
        private readonly BildeService _bildeService;
        private readonly LokasjonService _lokasjonService;

        public InnmeldingService(IConfiguration configuration, ILogger<InnmeldingService> logger, BildeService bildeService, LokasjonService lokasjonService)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
            _bildeService = bildeService;
            _lokasjonService = lokasjonService;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);



        // --- LAGER EN INNMELDING ---
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



        // --- HENTING AV ENKEL INNMELDIING ---
        public async Task<InnmeldingViewModel> GetInnmeldingById(int id)
        {
            try 
            {
                using var connection = new MySqlConnection(_connectionString);
                
                string sql = @"
                    SELECT 
                        i.innmeldingId,
                        i.brukerId,
                        i.kommuneId,
                        i.lokasjonId,
                        i.beskrivelse,
                        i.kommentar,
                        i.status,
                        i.opprettetDato,
                        i.saksbehandlerId,
                        i.bildeSti,
                        k.navn as kommuneNavn,
                        l.latitude,
                        l.longitude,
                        l.geoJson,
                        CONCAT(s.fornavn, ' ', s.etternavn) as saksbehandlerNavn
                    FROM Innmelding i 
                    INNER JOIN Kommune k ON i.kommuneId = k.kommuneId
                    INNER JOIN Lokasjon l ON i.lokasjonId = l.lokasjonId
                    LEFT JOIN Saksbehandler s ON i.saksbehandlerId = s.saksbehandlerId
                    WHERE i.innmeldingId = @Id";

                var innmelding = await connection.QueryFirstOrDefaultAsync<InnmeldingViewModel>(sql, new { Id = id });

                if (innmelding == null)
                {
                    _logger.LogWarning($"Fant ingen innmelding med ID {id}");
                    throw new KeyNotFoundException($"Innmelding med ID {id} finnes ikke");
                }

                innmelding.StatusClass = GetStatusClass(innmelding.Status);

                _logger.LogInformation($"Status: {innmelding.LokasjonId}");
                return innmelding;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, $"Database error ved henting av innmelding {id}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Uventet feil ved henting av innmelding {id}");
                throw;
            }
        }



        // --- HENTING AV FLERE INNMELDINGER ---
        public async Task<PagedResult<InnmeldingViewModel>> GetInnmeldinger(InnmeldingRequest request)
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
                    k.navn as kommuneNavn,
                    f.navn as fylkeNavn
                FROM Innmelding i
                INNER JOIN Kommune k ON i.kommuneId = k.kommuneId
                INNER JOIN Fylke f ON k.fylkeId = f.fylkeId";

            var whereConditions = BuildWhereConditions(request);
            var parameters = CreateParameters(request);

            if (whereConditions.Any())
            {
                sql += " WHERE " + string.Join(" AND ", whereConditions);
            }

            var totalItems = await GetTotalItems(connection, sql, parameters);
            sql = AddSortingAndPagination(sql, request);

            var innmeldinger = await connection.QueryAsync<InnmeldingViewModel>(sql, parameters);
            
            return new PagedResult<InnmeldingViewModel>
            {
                Items = ProcessItems(innmeldinger.ToList()),
                TotalItems = totalItems,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }

        private List<string> BuildWhereConditions(InnmeldingRequest request)
        {
            var conditions = new List<string>();
            
            if (request.SaksbehandlerId.HasValue)
                conditions.Add("i.saksbehandlerId = @SaksbehandlerId");
            
            if (request.InnmelderBrukerId.HasValue)
                conditions.Add("i.brukerId = @InnmelderBrukerId");
            
            if (!string.IsNullOrEmpty(request.StatusFilter))
                conditions.Add("i.status = @Status");
            
            if (!string.IsNullOrEmpty(request.FylkeFilter))
                conditions.Add("f.navn = @FylkeFilter");

            if (!string.IsNullOrEmpty(request.KommuneFilter))
                conditions.Add("k.navn = @KommuneFilter");
            
            return conditions;
        }

        private DynamicParameters CreateParameters(InnmeldingRequest request)
        {
            var parameters = new DynamicParameters();
            
            if (request.SaksbehandlerId.HasValue)
                parameters.Add("SaksbehandlerId", request.SaksbehandlerId.Value);
            
            if (request.InnmelderBrukerId.HasValue)
                parameters.Add("InnmelderBrukerId", request.InnmelderBrukerId.Value);
            
            if (!string.IsNullOrEmpty(request.StatusFilter))
                parameters.Add("Status", request.StatusFilter);
            
            if (!string.IsNullOrEmpty(request.FylkeFilter))
                parameters.Add("FylkeFilter", request.FylkeFilter);
            
            if (!string.IsNullOrEmpty(request.KommuneFilter))
                parameters.Add("KommuneFilter", request.KommuneFilter);
            
            parameters.Add("Skip", (request.Page - 1) * request.PageSize);
            parameters.Add("Take", request.PageSize);
            
            return parameters;
        }

        private string AddSortingAndPagination(string sql, InnmeldingRequest request)
        {
            sql += request.SortOrder switch
            {
                "date_asc" => " ORDER BY i.opprettetDato ASC",
                _ => " ORDER BY i.opprettetDato DESC"
            };
            
            sql += " LIMIT @Skip, @Take";
            return sql;
        }

        private async Task<int> GetTotalItems(MySqlConnection connection, string sql, DynamicParameters parameters)
        {
            var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
            return await connection.ExecuteScalarAsync<int>(countSql, parameters);
        }



        // --- OPPDATERINGER ---

        // Klasser for oppdatering
        private List<InnmeldingViewModel> ProcessItems(List<InnmeldingViewModel> items)
        {
            return items.Select(i => new InnmeldingViewModel
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
        }


        public class InnmeldingUpdateModel
        {
            public int InnmeldingId { get; set; }
            public string? Status { get; set; }
            public int? SaksbehandlerId { get; set; }
            public string? Beskrivelse { get; set; }
            public string? Kommentar { get; set; }
            public string? BildeSti { get; set; }
            public DateTime? OppdatertDato { get; set; }
        }

        private async Task<bool> UpdateInnmeldingInternal(InnmeldingUpdateModel updateModel)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var updateFields = new List<string>();
                var parameters = new DynamicParameters();
                
                // Legg til felter som skal oppdateres
                if (updateModel.Status != null)
                {
                    updateFields.Add("status = @Status");
                    parameters.Add("Status", updateModel.Status);
                }
                
                if (updateModel.SaksbehandlerId.HasValue)
                {
                    updateFields.Add("saksbehandlerId = @SaksbehandlerId");
                    parameters.Add("SaksbehandlerId", updateModel.SaksbehandlerId.Value);
                }
                
                if (updateModel.Beskrivelse != null)
                {
                    updateFields.Add("beskrivelse = @Beskrivelse");
                    parameters.Add("Beskrivelse", updateModel.Beskrivelse);
                }
                
                if (updateModel.Kommentar != null)
                {
                    updateFields.Add("kommentar = @Kommentar");
                    parameters.Add("Kommentar", updateModel.Kommentar);
                }
                
                if (updateModel.BildeSti != null)
                {
                    updateFields.Add("bildeSti = @BildeSti");
                    parameters.Add("BildeSti", updateModel.BildeSti);
                }

                // Alltid oppdater oppdatertDato
                updateFields.Add("oppdatertDato = @OppdatertDato");
                parameters.Add("OppdatertDato", updateModel.OppdatertDato ?? DateTime.Now);
                parameters.Add("InnmeldingId", updateModel.InnmeldingId);

                if (!updateFields.Any()) return true; // Ingen endringer å gjøre

                string sql = $@"
                    UPDATE Innmelding 
                    SET {string.Join(", ", updateFields)}
                    WHERE innmeldingId = @InnmeldingId";

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating innmelding: {ex.Message}");
                return false;
            }
        }

        public async Task UpdateInnmeldingDetails(InnmeldingViewModel innmelding, LokasjonViewModel? lokasjon = null, IFormFile? bilde = null)
        {
            try 
            {
                var oppdatertDato = DateTime.Now;

                if (bilde != null)
                {
                    innmelding.BildeSti = await _bildeService.LagreBilde(bilde, innmelding.InnmeldingId);
                }

                if (lokasjon != null)
                {
                    await _lokasjonService.UpdateLokasjon(lokasjon, oppdatertDato);
                }

                await UpdateInnmeldingInternal(new InnmeldingUpdateModel
                {
                    InnmeldingId = innmelding.InnmeldingId,
                    Beskrivelse = innmelding.Beskrivelse,
                    BildeSti = innmelding.BildeSti,
                    OppdatertDato = oppdatertDato
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding detaljer for innmelding {InnmeldingId}", innmelding.InnmeldingId);
                throw;
            }
        }

        public async Task<bool> UpdateInnmeldingStatus(int innmeldingId, string status, int? saksbehandlerId = null)
        {
            return await UpdateInnmeldingInternal(new InnmeldingUpdateModel
            {
                InnmeldingId = innmeldingId,
                Status = status,
                SaksbehandlerId = saksbehandlerId
            });
        }

        public async Task<bool> UpdateInnmeldingSaksbehandler(int innmeldingId, int saksbehandlerId)
        {
            return await UpdateInnmeldingInternal(new InnmeldingUpdateModel
            {
                InnmeldingId = innmeldingId,
                SaksbehandlerId = saksbehandlerId,
                Status = "Under behandling"
            });
        }

        public async Task<bool> UpdateStatusAndKommentar(int innmeldingId, string kommentar, string? status = "under behandling")
        {
            return await UpdateInnmeldingInternal(new InnmeldingUpdateModel
            {
                InnmeldingId = innmeldingId,
                Status = status,
                Kommentar = kommentar
            });
        }

        public async Task<bool> UpdateInnmelding(Innmelding innmelding)
        {
            return await UpdateInnmeldingInternal(new InnmeldingUpdateModel
            {
                InnmeldingId = innmelding.InnmeldingId,
                Status = innmelding.Status,
                SaksbehandlerId = innmelding.SaksbehandlerId,
                OppdatertDato = DateTime.Now
            });
        }

        public async Task UpdateBildeSti(int innmeldingId, string bildeSti)
        {
            await UpdateInnmeldingInternal(new InnmeldingUpdateModel
            {
                InnmeldingId = innmeldingId,
                BildeSti = bildeSti,
                OppdatertDato = DateTime.Now
            });
        }

        private string GetStatusClass(string status) => status switch
        {
            "Ny" => "bg-blue-100 text-blue-800",
            "Under behandling" => "bg-yellow-100 text-yellow-800",
            "Godkjent" => "bg-green-100 text-green-800",
            "Avvist" => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };

        public List<string> GetAllStatuses()
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