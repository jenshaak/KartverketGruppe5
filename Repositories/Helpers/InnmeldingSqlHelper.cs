using Dapper;
using MySqlConnector;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Models.Helpers;

namespace KartverketGruppe5.Repositories.Helpers
{
    internal static class InnmeldingSqlHelper
    {
        internal static List<string> BuildWhereConditions(InnmeldingRequest request)
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

        internal static DynamicParameters CreateParameters(InnmeldingRequest request)
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

        internal static string AddSortingAndPagination(string sql, InnmeldingRequest request)
        {
            sql += request.SortOrder switch
            {
                "date_asc" => " ORDER BY i.opprettetDato ASC",
                _ => " ORDER BY i.opprettetDato DESC"
            };
            
            sql += " LIMIT @Skip, @Take";
            return sql;
        }

        internal static async Task<int> GetTotalItems(MySqlConnection connection, string sql, DynamicParameters parameters)
        {
            var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
            return await connection.ExecuteScalarAsync<int>(countSql, parameters);
        }

        internal static List<InnmeldingViewModel> ProcessItems(List<InnmeldingViewModel> items)
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
                StatusClass = InnmeldingHelper.GetStatusClass(i.Status)
            }).ToList();
        }
    }
} 