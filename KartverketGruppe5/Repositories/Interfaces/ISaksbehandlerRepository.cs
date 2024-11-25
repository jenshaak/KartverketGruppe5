using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.Interfaces;

namespace KartverketGruppe5.Repositories.Interfaces
{
    public interface ISaksbehandlerRepository
    {
        Task<Saksbehandler?> GetSaksbehandlerById(int id);
        Task<Saksbehandler?> GetSaksbehandlerByEmail(string email);
        Task<IPagedResult<Saksbehandler>> GetAllSaksbehandlere(string sortOrder, int page);
        Task<bool> CreateSaksbehandler(Saksbehandler saksbehandler);
        Task<bool> UpdateSaksbehandler(SaksbehandlerRegistrerViewModel saksbehandler);
        Task<bool> Delete(int saksbehandlerId);
        Task<List<Saksbehandler>> SokSaksbehandlere(string sokestreng);
    }
} 