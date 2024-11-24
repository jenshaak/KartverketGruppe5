using KartverketGruppe5.Models; 
using KartverketGruppe5.Models.Interfaces;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Models.Helpers;

namespace KartverketGruppe5.Services.Interfaces;

public interface IInnmeldingService
{
    // Grunnleggende CRUD-operasjoner
    Task<int> CreateInnmelding(int brukerId, int kommuneId, int lokasjonId, string beskrivelse, string? bildeSti);

    // Hent en innmelding
    Task<InnmeldingViewModel> GetInnmeldingById(int id);

    // Hent alle innmeldinger
    Task<IPagedResult<InnmeldingViewModel>> GetInnmeldinger(InnmeldingRequest request);

    // Slett innmelding
    Task SlettInnmelding(int id);
    // Status og oppdateringer
    Task<bool> UpdateInnmeldingStatus(int innmeldingId, string status, int? saksbehandlerId = null);
    Task<bool> UpdateInnmeldingSaksbehandler(int innmeldingId, int saksbehandlerId);
    Task<bool> UpdateStatusAndKommentar(int innmeldingId, string kommentar, string? status = "under behandling");
    Task<bool> UpdateInnmelding(Innmelding innmelding);
    Task UpdateBildeSti(int innmeldingId, string bildeSti);
    Task UpdateInnmeldingDetails(InnmeldingViewModel innmelding, LokasjonViewModel? lokasjon = null, IFormFile? bilde = null);
} 