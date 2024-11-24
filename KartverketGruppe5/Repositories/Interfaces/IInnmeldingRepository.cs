using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.Interfaces;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Models;

namespace KartverketGruppe5.Repositories.Interfaces
{
    public interface IInnmeldingRepository
    {
        Task<int> AddInnmelding(int brukerId, int kommuneId, int lokasjonId, string beskrivelse, string? bildeSti);
        Task<InnmeldingViewModel?> GetInnmeldingById(int id);
        Task<IPagedResult<InnmeldingViewModel>> GetInnmeldinger(InnmeldingRequest request);
        Task SlettInnmelding(int id);
        Task<bool> UpdateInnmelding(InnmeldingUpdateModel updateModel);
        Task UpdateBildeSti(int innmeldingId, string bildeSti);
    }
} 