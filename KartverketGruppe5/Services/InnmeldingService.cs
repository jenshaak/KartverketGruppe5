using Dapper;
using MySqlConnector;
using System.Data;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models.Interfaces;
using KartverketGruppe5.Repositories.Interfaces;
using KartverketGruppe5.Models.Helpers;
namespace KartverketGruppe5.Services
{
    /// <summary>
    /// Service for innmeldinger
    /// </summary>
    public class InnmeldingService : IInnmeldingService
    {
        private readonly IInnmeldingRepository _repository;
        private readonly ILogger<InnmeldingService> _logger;
        private readonly IBildeService _bildeService;
        private readonly ILokasjonService _lokasjonService;

        public InnmeldingService(
            IInnmeldingRepository repository,
            ILogger<InnmeldingService> logger,
            IBildeService bildeService,
            ILokasjonService lokasjonService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bildeService = bildeService ?? throw new ArgumentNullException(nameof(bildeService));
            _lokasjonService = lokasjonService ?? throw new ArgumentNullException(nameof(lokasjonService));
        }

        /// <summary>
        /// Oppretter en ny innmelding
        /// </summary>
        public async Task<int> CreateInnmelding(int brukerId, int kommuneId, int lokasjonId, string beskrivelse, string? bildeSti)
        {
            try
            {
                return await _repository.AddInnmelding(brukerId, kommuneId, lokasjonId, beskrivelse, bildeSti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved opprettelse av innmelding for bruker {BrukerId}", brukerId);
                throw;
            }
        }

        /// <summary>
        /// Henter en spesifikk innmelding med full informasjon
        /// </summary>
        public async Task<InnmeldingViewModel> GetInnmeldingById(int id)
        {
            var innmelding = await _repository.GetInnmeldingById(id);
            if (innmelding == null)
            {
                _logger.LogWarning("Innmelding med id {InnmeldingId} ble ikke funnet", id);
                throw new KeyNotFoundException($"Innmelding med id {id} ble ikke funnet");
            }
            return innmelding;
        }

        /// <summary>
        /// Henter innmeldinger basert på søkekriterier
        /// </summary>
        public async Task<IPagedResult<InnmeldingViewModel>> GetInnmeldinger(InnmeldingRequest request)
        {
            try
            {
                return await _repository.GetInnmeldinger(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av innmeldinger");
                throw;
            }
        }

        /// <summary>
        /// Sletter en innmelding
        /// </summary>
        public async Task SlettInnmelding(int id)
        {
            try
            {
                await _repository.SlettInnmelding(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved sletting av innmelding {InnmeldingId}", id);
                throw;
            }
        }
        

        /// <summary>
        /// Oppdaterer en eksisterende innmelding
        /// </summary>
        public async Task<bool> UpdateInnmelding(InnmeldingUpdateModel updateModel)
        {
            try
            {
                updateModel.OppdatertDato = DateTime.Now;
                return await _repository.UpdateInnmelding(updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding {InnmeldingId}", updateModel.InnmeldingId);
                throw;
            }
        }

        /// <summary>
        /// Oppdaterer bilde for en innmelding
        /// </summary>
        public async Task UpdateBilde(int innmeldingId, IFormFile bilde)
        {
            try
            {
                var innmelding = await GetInnmeldingById(innmeldingId);
                
                // Lagre nytt bilde
                var nyBildeSti = await _bildeService.LagreBilde(bilde, innmeldingId);
                if (nyBildeSti != null)
                {
                    await _repository.UpdateBildeSti(innmeldingId, nyBildeSti);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av bilde for innmelding {InnmeldingId}", innmeldingId);
                throw;
            }
        }

        /// <summary>
        /// Oppdaterer status og/eller saksbehandler for en innmelding
        /// </summary>
        public async Task<bool> UpdateInnmeldingStatus(
            int innmeldingId, 
            int? saksbehandlerId = null, 
            string? status = null)
        {
            try
            {
                var updateModel = new InnmeldingUpdateModel
                {
                    InnmeldingId = innmeldingId,
                    SaksbehandlerId = saksbehandlerId,
                    // Hvis status ikke er spesifisert og vi har en saksbehandler, sett til "Under behandling"
                    Status = status ?? (saksbehandlerId.HasValue ? "Under behandling" : null),
                    OppdatertDato = DateTime.Now
                };

                _logger.LogInformation(
                    "Oppdaterer innmelding {InnmeldingId} - Status: {Status}, SaksbehandlerId: {SaksbehandlerId}", 
                    innmeldingId, updateModel.Status, saksbehandlerId);

                return await _repository.UpdateInnmelding(updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Feil ved oppdatering av innmelding {InnmeldingId}. Status: {Status}, SaksbehandlerId: {SaksbehandlerId}", 
                    innmeldingId, status, saksbehandlerId);
                throw;
            }
        }

        public async Task<bool> UpdateStatusAndKommentar(int innmeldingId, string kommentar, string? status = "Under behandling")
        {
            var updateModel = new InnmeldingUpdateModel
            {
                InnmeldingId = innmeldingId,
                Status = status,
                Kommentar = kommentar,
                OppdatertDato = DateTime.Now
            };
            
            return await _repository.UpdateInnmelding(updateModel);
        }

        public async Task<bool> UpdateInnmelding(Innmelding innmelding)
        {
            try
            {
                var updateModel = new InnmeldingUpdateModel
                {
                    InnmeldingId = innmelding.InnmeldingId,
                    Status = innmelding.Status,
                    SaksbehandlerId = innmelding.SaksbehandlerId,
                    Beskrivelse = innmelding.Beskrivelse,
                    Kommentar = innmelding.Kommentar,
                    BildeSti = innmelding.BildeSti,
                    OppdatertDato = DateTime.Now
                };
                return await _repository.UpdateInnmelding(updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding {InnmeldingId}", innmelding.InnmeldingId);
                throw;
            }
        }

        public async Task UpdateBildeSti(int innmeldingId, string bildeSti)
        {
            try
            {
                await _repository.UpdateBildeSti(innmeldingId, bildeSti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av bildesti for innmelding {InnmeldingId}", innmeldingId);
                throw;
            }
        }

        public async Task UpdateInnmeldingDetails(InnmeldingViewModel innmelding, LokasjonViewModel? lokasjon = null, IFormFile? bilde = null)
        {
            try
            {
                var oppdatertDato = DateTime.Now;

                // Håndter bilde hvis det er lastet opp
                if (bilde != null)
                {
                    innmelding.BildeSti = await _bildeService.LagreBilde(bilde, innmelding.InnmeldingId);
                }

                if (lokasjon != null)
                {
                    await _lokasjonService.UpdateLokasjon(lokasjon, oppdatertDato);
                }

                // Oppdater innmelding til slutt
                var updateModel = new InnmeldingUpdateModel
                {
                    InnmeldingId = innmelding.InnmeldingId,
                    Beskrivelse = innmelding.Beskrivelse,
                    BildeSti = innmelding.BildeSti,
                    OppdatertDato = oppdatertDato
                };

                await _repository.UpdateInnmelding(updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding detaljer for innmelding {InnmeldingId}", innmelding.InnmeldingId);
                throw;
            }
        }

    }
} 