using KartverketGruppe5.API_Models;

namespace KartverketGruppe5.Services
{
    public interface IStedsnavnService
    {
        Task<StedsnavnResponse> GetStedsnavnAsync(string search);
    }
}
