using MySqlConnector;
using Dapper;
using KartverketGruppe5.Models;
using System.Security.Cryptography;
using System.Text;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models.Interfaces;

namespace KartverketGruppe5.Services
{
    public class SaksbehandlerService : ISaksbehandlerService
    {
        private readonly string _connectionString;
        private readonly ILogger<SaksbehandlerService> _logger;
        private readonly IPasswordService _passwordService;

        public SaksbehandlerService(IConfiguration configuration, ILogger<SaksbehandlerService> logger, IPasswordService passwordService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
            _passwordService = passwordService;
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
                _logger.LogError(ex, "Error getting saksbehandler with id {SaksbehandlerId}", id);
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
                _logger.LogError(ex, "Error getting saksbehandler by email {Email}", email);
                throw;
            }
        }

        public async Task<IPagedResult<Saksbehandler>> GetAllSaksbehandlere(
            string sortOrder = PagedResult<Saksbehandler>.DefaultSortOrder, 
            int page = PagedResult<Saksbehandler>.DefaultPage)
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
                _logger.LogError(ex, "Error getting all saksbehandlere with sort order {SortOrder} and page {Page}", 
                    sortOrder, page);
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

                saksbehandler.Passord = _passwordService.HashPassword(saksbehandler.Passord);
                saksbehandler.OpprettetDato = DateTime.Now;

                var rowsAffected = await connection.ExecuteAsync(sql, saksbehandler);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating saksbehandler with email {Email}", saksbehandler.Email);
                throw;
            }
        }

        public async Task<bool> UpdateSaksbehandler(SaksbehandlerRegistrerViewModel saksbehandler)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                
                if (string.IsNullOrEmpty(saksbehandler.Passord))
                {
                    var existing = await GetSaksbehandlerById(saksbehandler.SaksbehandlerId);
                    if (existing == null) return false;
                    saksbehandler.Passord = existing.Passord;
                }
                else
                {
                    saksbehandler.Passord = _passwordService.HashPassword(saksbehandler.Passord);
                }

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
                _logger.LogError(ex, "Error updating saksbehandler with id {SaksbehandlerId}", 
                    saksbehandler.SaksbehandlerId);
                throw;
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return _passwordService.VerifyPassword(password, hashedPassword);
        }
    }
} 