using KartverketGruppe5.API_Models;

namespace KartverketGruppe5.Services
{
    public interface IKommuneInfoService
    {
        Task<KommuneInfo> GetKommuneInfoAsync(string kommuneNr);
    }
}
