using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.Interfaces;

namespace KartverketGruppe5.Services.Interfaces;

public interface ISaksbehandlerService
{
    Task<IPagedResult<Saksbehandler>> GetAllSaksbehandlere(
        string sortOrder = PagedResult<Saksbehandler>.DefaultSortOrder, 
        int page = PagedResult<Saksbehandler>.DefaultPage);
    Task<Saksbehandler?> GetSaksbehandlerById(int id);
    Task<Saksbehandler?> GetSaksbehandlerByEmail(string email);
    Task<bool> CreateSaksbehandler(Saksbehandler saksbehandler);
    Task<bool> UpdateSaksbehandler(SaksbehandlerRegistrerViewModel saksbehandler);
    Task<bool> DeleteSaksbehandler(int saksbehandlerId);
    Task<List<Saksbehandler>> SokSaksbehandlere(string sokestreng);
} 