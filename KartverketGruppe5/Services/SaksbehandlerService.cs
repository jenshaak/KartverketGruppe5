using MySqlConnector;
using Dapper;
using KartverketGruppe5.Models;
using System.Security.Cryptography;
using System.Text;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models.Interfaces;
using KartverketGruppe5.Repositories.Interfaces;

namespace KartverketGruppe5.Services
{
    /// <summary>
    /// Service for saksbehandlere
    /// </summary>
    public class SaksbehandlerService : ISaksbehandlerService
    {
        private readonly ISaksbehandlerRepository _repository;
        private readonly ILogger<SaksbehandlerService> _logger;
        private readonly IPasswordService _passwordService;

        public SaksbehandlerService(
            ISaksbehandlerRepository repository,
            ILogger<SaksbehandlerService> logger,
            IPasswordService passwordService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }

        /// <summary>
        /// Henter en saksbehandler basert på ID
        /// </summary>
        public async Task<Saksbehandler?> GetSaksbehandlerById(int id)
        {
            try
            {
                return await _repository.GetSaksbehandlerById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av saksbehandler med ID {SaksbehandlerId}", id);
                throw;
            }
        }

        /// <summary>
        /// Henter en saksbehandler basert på email
        /// </summary>
        public async Task<Saksbehandler?> GetSaksbehandlerByEmail(string email)
        {
            try
            {
                return await _repository.GetSaksbehandlerByEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av saksbehandler med email {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Henter alle saksbehandlere med paginering
        /// </summary>
        public async Task<IPagedResult<Saksbehandler>> GetAllSaksbehandlere(
            string sortOrder = PagedResult<Saksbehandler>.DefaultSortOrder, 
            int page = PagedResult<Saksbehandler>.DefaultPage)
        {
            try
            {
                return await _repository.GetAllSaksbehandlere(sortOrder, page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all saksbehandlere");
                throw;
            }
        }

        /// <summary>
        /// Oppretter en saksbehandler
        /// </summary>
        public async Task<bool> CreateSaksbehandler(Saksbehandler saksbehandler)
        {
            try
            {
                saksbehandler.Passord = _passwordService.HashPassword(saksbehandler.Passord);
                saksbehandler.OpprettetDato = DateTime.Now;
                
                return await _repository.CreateSaksbehandler(saksbehandler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating saksbehandler with email {Email}", saksbehandler.Email);
                throw;
            }
        }

        /// <summary>
        /// Oppdaterer en saksbehandler
        /// </summary>
        public async Task<bool> UpdateSaksbehandler(SaksbehandlerRegistrerViewModel saksbehandler)
        {
            try
            {
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

                return await _repository.UpdateSaksbehandler(saksbehandler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating saksbehandler with id {SaksbehandlerId}", 
                    saksbehandler.SaksbehandlerId);
                throw;
            }
        }

        /// <summary>
        /// Sletter en saksbehandler
        /// </summary>
        public async Task<bool> DeleteSaksbehandler(int saksbehandlerId)
        {
            try
            {
                return await _repository.Delete(saksbehandlerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved sletting av saksbehandler med ID: {Id}", saksbehandlerId);
                return false;
            }
        }

        public async Task<List<Saksbehandler>> SokSaksbehandlere(string sokestreng)
        {
            try
            {
                return await _repository.SokSaksbehandlere(sokestreng);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved søk etter saksbehandlere");
                throw;
            }
        }
    }
} 