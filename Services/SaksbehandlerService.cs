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
            _repository = repository;
            _logger = logger;
            _passwordService = passwordService;
        }

        public async Task<Saksbehandler?> GetSaksbehandlerById(int id)
        {
            try
            {
                return await _repository.GetSaksbehandlerById(id);
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
                return await _repository.GetSaksbehandlerByEmail(email);
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
                return await _repository.GetAllSaksbehandlere(sortOrder, page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all saksbehandlere");
                throw;
            }
        }

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

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return _passwordService.VerifyPassword(password, hashedPassword);
        }
    }
} 