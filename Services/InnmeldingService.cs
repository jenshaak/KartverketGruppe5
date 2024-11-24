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
            _repository = repository;
            _logger = logger;
            _bildeService = bildeService;
            _lokasjonService = lokasjonService;
        }

        public async Task<int> CreateInnmelding(int brukerId, int kommuneId, int lokasjonId, string beskrivelse, string? bildeSti)
        {
            try
            {
                return await _repository.AddInnmelding(brukerId, kommuneId, lokasjonId, beskrivelse, bildeSti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved opprettelse av innmelding");
                throw;
            }
        }


        public async Task<InnmeldingViewModel> GetInnmeldingById(int id)
        {
            var innmeldingViewModel = await _repository.GetInnmeldingById(id);
            if (innmeldingViewModel == null)
                throw new KeyNotFoundException($"Innmelding med id {id} ble ikke funnet");

            return innmeldingViewModel;
        }

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



        /// ----- OPPDATERINGER -----

        public async Task<bool> UpdateInnmelding(InnmeldingUpdateModel updateModel)
        {
            try
            {
                updateModel.OppdatertDato = DateTime.Now;
                return await _repository.UpdateInnmelding(updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding {InnmeldingId}", 
                    updateModel.InnmeldingId);
                throw;
            }
        }

        public async Task UpdateBilde(int innmeldingId, IFormFile bilde)
        {
            try
            {
                var innmelding = await GetInnmeldingById(innmeldingId);
                if (innmelding == null)
                {
                    throw new KeyNotFoundException($"Innmelding {innmeldingId} ikke funnet");
                }

                // Slett gammelt bilde hvis det finnes
                if (!string.IsNullOrEmpty(innmelding.BildeSti))
                {
                    // TODO: Implementer sletting av gammelt bilde
                }

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


        public async Task<bool> UpdateInnmeldingStatus(int innmeldingId, string status, int? saksbehandlerId)
        {
            try
            {
                var updateModel = new InnmeldingUpdateModel
                {
                    InnmeldingId = innmeldingId,
                    Status = status,
                    SaksbehandlerId = saksbehandlerId,
                    OppdatertDato = DateTime.Now
                };

                return await _repository.UpdateInnmelding(updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av status for innmelding {InnmeldingId}", innmeldingId);
                throw;
            }
        }

        public async Task<bool> UpdateInnmeldingSaksbehandler(int innmeldingId, int saksbehandlerId)
        {
            try
            {
                var updateModel = new InnmeldingUpdateModel
                {
                    InnmeldingId = innmeldingId,
                    SaksbehandlerId = saksbehandlerId,
                    Status = "Under behandling",
                    OppdatertDato = DateTime.Now
                };

                return await _repository.UpdateInnmelding(updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av saksbehandler for innmelding {InnmeldingId}", innmeldingId);
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