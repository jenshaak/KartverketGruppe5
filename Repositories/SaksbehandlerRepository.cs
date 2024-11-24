using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Repositories.Interfaces;
using MySqlConnector;
using Dapper;
using KartverketGruppe5.Models.Interfaces;
namespace KartverketGruppe5.Repositories
{
    public class SaksbehandlerRepository : ISaksbehandlerRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<SaksbehandlerRepository> _logger;

        public SaksbehandlerRepository(IConfiguration configuration, ILogger<SaksbehandlerRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<Saksbehandler?> GetSaksbehandlerById(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT 
                        saksbehandlerId,
                        fornavn,
                        etternavn,
                        email,
                        passord,
                        admin,
                        opprettetDato
                    FROM Saksbehandler 
                    WHERE saksbehandlerId = @Id";
                
                return await connection.QueryFirstOrDefaultAsync<Saksbehandler>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av saksbehandler {SaksbehandlerId}", id);
                throw;
            }
        }

        public async Task<Saksbehandler?> GetSaksbehandlerByEmail(string email)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT 
                        saksbehandlerId,
                        fornavn,
                        etternavn,
                        email,
                        passord,
                        admin,
                        opprettetDato
                    FROM Saksbehandler 
                    WHERE email = @Email";
                
                return await connection.QueryFirstOrDefaultAsync<Saksbehandler>(sql, new { Email = email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av saksbehandler med email {Email}", email);
                throw;
            }
        }

        public async Task<IPagedResult<Saksbehandler>> GetAllSaksbehandlere(string sortOrder, int page)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                
                const string countSql = "SELECT COUNT(*) FROM Saksbehandler";
                var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

                var pagedResult = new PagedResult<Saksbehandler>
                {
                    TotalItems = totalItems,
                    CurrentPage = page,
                    Items = new List<Saksbehandler>()
                };

                var orderByClause = GetOrderByClause(sortOrder);

                var sql = $@"
                    SELECT 
                        saksbehandlerId,
                        fornavn,
                        etternavn,
                        email,
                        passord,
                        admin,
                        opprettetDato
                    FROM Saksbehandler
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

        private static string GetOrderByClause(string sortOrder) => sortOrder switch
        {
            "admin_desc" => "admin DESC, opprettetDato DESC",
            "admin_asc" => "admin ASC, opprettetDato DESC",
            "date_asc" => "opprettetDato ASC",
            _ => "opprettetDato DESC"
        };
    }
} 