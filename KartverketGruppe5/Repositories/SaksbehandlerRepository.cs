using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Repositories.Interfaces;
using MySqlConnector;
using Dapper;
using KartverketGruppe5.Models.Interfaces;
namespace KartverketGruppe5.Repositories
{
    /// <summary>
    /// Repository for saksbehandlere
    /// </summary>
    public class SaksbehandlerRepository : ISaksbehandlerRepository
    {
        private readonly string _connectionString;
        // private readonly IDbContext _dbContext;
        private readonly ILogger<SaksbehandlerRepository> _logger;

        // SQL-spørringer som konstanter for bedre vedlikehold
        private const string SELECT_SAKSBEHANDLER_BASE = @"
            SELECT 
                saksbehandlerId,
                fornavn,
                etternavn,
                email,
                passord,
                admin,
                opprettetDato
            FROM Saksbehandler";

        public SaksbehandlerRepository(
            IConfiguration configuration, 
            ILogger<SaksbehandlerRepository> logger
            // IDbContext dbContext
            )
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Henter saksbehandler basert på ID
        /// </summary>
        public async Task<Saksbehandler?> GetSaksbehandlerById(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var sql = $"{SELECT_SAKSBEHANDLER_BASE} WHERE saksbehandlerId = @Id";
                
                return await connection.QueryFirstOrDefaultAsync<Saksbehandler>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av saksbehandler {SaksbehandlerId}", id);
                throw;
            }
        }

        /// <summary>
        /// Henter saksbehandler basert på email
        /// </summary>
        public async Task<Saksbehandler?> GetSaksbehandlerByEmail(string email)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var sql = $"{SELECT_SAKSBEHANDLER_BASE} WHERE email = @Email";
                
                return await connection.QueryFirstOrDefaultAsync<Saksbehandler>(sql, new { Email = email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av saksbehandler med email {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Henter alle saksbehandlere med paginering og sortering
        /// </summary>
        public async Task<IPagedResult<Saksbehandler>> GetAllSaksbehandlere(string sortOrder, int page)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                
                // Hent totalt antall saksbehandlere
                const string countSql = "SELECT COUNT(*) FROM Saksbehandler";
                var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

                // Opprett paginert resultat
                var pagedResult = new PagedResult<Saksbehandler>
                {
                    TotalItems = totalItems,
                    CurrentPage = page,
                    Items = new List<Saksbehandler>()
                };

                // Hvis ingen resultater, returner tom liste
                if (totalItems == 0) return pagedResult;

                // Hent saksbehandlere for gjeldende side
                var orderByClause = GetOrderByClause(sortOrder);
                var sql = $@"{SELECT_SAKSBEHANDLER_BASE} 
                            ORDER BY {orderByClause} 
                            LIMIT @Skip, @Take";

                var items = await connection.QueryAsync<Saksbehandler>(sql, new
                {
                    Skip = pagedResult.Skip,
                    Take = pagedResult.Take
                });

                pagedResult.Items = items.ToList();
                return pagedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av alle saksbehandlere");
                throw;
            }
        }

        /// <summary>
        /// Oppretter ny saksbehandler
        /// </summary>
        public async Task<bool> CreateSaksbehandler(Saksbehandler saksbehandler)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    INSERT INTO Saksbehandler (
                        fornavn, 
                        etternavn, 
                        email, 
                        passord, 
                        admin, 
                        opprettetDato
                    ) VALUES (
                        @Fornavn, 
                        @Etternavn, 
                        @Email, 
                        @Passord, 
                        @Admin, 
                        @OpprettetDato
                    )";

                var rowsAffected = await connection.ExecuteAsync(sql, saksbehandler);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved opprettelse av saksbehandler {Email}", 
                    saksbehandler.Email);
                throw;
            }
        }

        /// <summary>
        /// Oppdaterer eksisterende saksbehandler
        /// </summary>
        public async Task<bool> UpdateSaksbehandler(SaksbehandlerRegistrerViewModel saksbehandler)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    UPDATE Saksbehandler 
                    SET 
                        fornavn = @Fornavn,
                        etternavn = @Etternavn,
                        email = @Email,
                        passord = @Passord,
                        admin = @Admin
                    WHERE saksbehandlerId = @SaksbehandlerId";

                var rowsAffected = await connection.ExecuteAsync(sql, saksbehandler);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved oppdatering av saksbehandler {SaksbehandlerId}", 
                    saksbehandler.SaksbehandlerId);
                throw;
            }
        }

        /// <summary>
        /// Sletter en saksbehandler og nullstiller referanser i Innmelding-tabellen
        /// </summary>
        public async Task<bool> Delete(int saksbehandlerId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();

                // Først, oppdater alle innmeldinger som refererer til denne saksbehandleren
                const string updateInnmeldingSql = @"
                    UPDATE Innmelding 
                    SET saksbehandlerId = NULL,
                        status = CASE 
                            WHEN status = 'Under behandling' THEN 'Ny'
                            ELSE status 
                        END
                    WHERE saksbehandlerId = @SaksbehandlerId";

                await connection.ExecuteAsync(updateInnmeldingSql, new { SaksbehandlerId = saksbehandlerId }, transaction);

                // Deretter, slett saksbehandleren
                const string deleteSql = "DELETE FROM Saksbehandler WHERE saksbehandlerId = @SaksbehandlerId";
                var affected = await connection.ExecuteAsync(deleteSql, new { SaksbehandlerId = saksbehandlerId }, transaction);

                await transaction.CommitAsync();
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved sletting av saksbehandler {SaksbehandlerId}", 
                    saksbehandlerId);
                throw;
            }
        }

        /// <summary>
        /// Genererer SQL ORDER BY clause basert på sorteringsparameter
        /// </summary>
        private static string GetOrderByClause(string sortOrder) => sortOrder switch
        {
            "admin_desc" => "admin DESC, opprettetDato DESC",
            "admin_asc" => "admin ASC, opprettetDato DESC",
            "date_asc" => "opprettetDato ASC",
            _ => "opprettetDato DESC"
        };
    }
} 