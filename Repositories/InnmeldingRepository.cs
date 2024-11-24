using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Models.Helpers;
using KartverketGruppe5.Models.Interfaces;
using KartverketGruppe5.Repositories.Interfaces;
using KartverketGruppe5.Repositories.Helpers;
using MySqlConnector;
using Dapper;
using System.Collections.Generic;
using System;
using System.Data;

namespace KartverketGruppe5.Repositories
{
    public class InnmeldingRepository : IInnmeldingRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<InnmeldingRepository> _logger;

        public InnmeldingRepository(IConfiguration configuration, ILogger<InnmeldingRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }



        /// ----- LAGRING AV INNMELDING -----
        public async Task<int> AddInnmelding(int brukerId, int kommuneId, int lokasjonId, string beskrivelse, string? bildeSti)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    INSERT INTO Innmelding (
                        brukerId, 
                        kommuneId, 
                        lokasjonId, 
                        beskrivelse, 
                        bildeSti,
                        status,
                        opprettetDato
                    ) VALUES (
                        @BrukerId,
                        @KommuneId,
                        @LokasjonId,
                        @Beskrivelse,
                        @BildeSti,
                        'Ny',
                        @OpprettetDato
                    );
                    SELECT LAST_INSERT_ID();";

                var parameters = new
                {
                    BrukerId = brukerId,
                    KommuneId = kommuneId,
                    LokasjonId = lokasjonId,
                    Beskrivelse = beskrivelse,
                    BildeSti = bildeSti,
                    OpprettetDato = DateTime.Now
                };

                return await connection.ExecuteScalarAsync<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved opprettelse av innmelding");
                throw;
            }
        }



        /// ----- HENTING AV INNMELDINGER -----
        public async Task<InnmeldingViewModel?> GetInnmeldingById(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT 
                        i.innmeldingId,
                        i.brukerId,
                        i.kommuneId,
                        i.lokasjonId,
                        i.beskrivelse,
                        i.kommentar,
                        i.status,
                        i.opprettetDato,
                        i.oppdatertDato,
                        i.saksbehandlerId,
                        i.bildeSti,
                        k.navn as kommuneNavn,
                        f.navn as fylkeNavn,
                        CONCAT(s.fornavn, ' ', s.etternavn) as saksbehandlerNavn
                    FROM Innmelding i
                    LEFT JOIN Kommune k ON i.kommuneId = k.kommuneId
                    LEFT JOIN Fylke f ON k.fylkeId = f.fylkeId
                    LEFT JOIN Saksbehandler s ON i.saksbehandlerId = s.saksbehandlerId
                    WHERE i.innmeldingId = @Id";

                var innmelding = await connection.QuerySingleOrDefaultAsync<InnmeldingViewModel>(sql, new { Id = id });
                
                if (innmelding != null)
                {
                    innmelding.StatusClass = InnmeldingHelper.GetStatusClass(innmelding.Status);
                    return innmelding;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av innmelding {InnmeldingId}", id);
                throw;
            }
        }

        /// ----- HENTING AV INNMELDINGER -----
        public async Task<IPagedResult<InnmeldingViewModel>> GetInnmeldinger(InnmeldingRequest request)
        {
            try
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

                var whereConditions = InnmeldingSqlHelper.BuildWhereConditions(request);
                var parameters = InnmeldingSqlHelper.CreateParameters(request);

                if (whereConditions.Any())
                {
                    sql += " WHERE " + string.Join(" AND ", whereConditions);
                }

                var totalItems = await InnmeldingSqlHelper.GetTotalItems(connection, sql, parameters);
                sql = InnmeldingSqlHelper.AddSortingAndPagination(sql, request);

                var innmeldinger = await connection.QueryAsync<InnmeldingViewModel>(sql, parameters);
                
                return new PagedResult<InnmeldingViewModel>
                {
                    Items = InnmeldingSqlHelper.ProcessItems(innmeldinger.ToList()),
                    TotalItems = totalItems,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av innmeldinger");
                throw;
            }
        }


        /// ----- SLETTING AV INNMELDINGER -----
        public async Task SlettInnmelding(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = "DELETE FROM Innmelding WHERE innmeldingId = @Id";
                
                await connection.ExecuteAsync(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved sletting av innmelding {InnmeldingId}", id);
                throw;
            }
        }



        /// ----- OPPDATERING AV INNMELDINGER -----
        public async Task<bool> UpdateInnmelding(InnmeldingUpdateModel updateModel)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var updateFields = new List<string>();
                var parameters = new DynamicParameters();
                
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

                if (!updateFields.Any()) return true;

                string sql = $@"
                    UPDATE Innmelding 
                    SET {string.Join(", ", updateFields)}
                    WHERE innmeldingId = @InnmeldingId";

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved oppdatering av innmelding {InnmeldingId}", 
                    updateModel.InnmeldingId);
                throw;
            }
        }

        public async Task UpdateBildeSti(int innmeldingId, string bildeSti)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    UPDATE Innmelding 
                    SET bildeSti = @BildeSti 
                    WHERE innmeldingId = @InnmeldingId";

                await connection.ExecuteAsync(sql, new { InnmeldingId = innmeldingId, BildeSti = bildeSti });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved oppdatering av bildesti for innmelding {InnmeldingId}", 
                    innmeldingId);
                throw;
            }
        }


    }
} 