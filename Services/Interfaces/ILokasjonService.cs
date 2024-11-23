using KartverketGruppe5.Models.ViewModels;

namespace KartverketGruppe5.Services.Interfaces;

public interface ILokasjonService
{
    Task<List<LokasjonViewModel>> GetAllLokasjoner();
    Task<int> AddLokasjon(string geoJson, double latitude, double longitude, string geometriType);
    Task<LokasjonViewModel?> GetLokasjonById(int id);
    Task<int> GetKommuneIdFromCoordinates(double latitude, double longitude);
    Task UpdateLokasjon(LokasjonViewModel lokasjon, DateTime oppdatertDato);
} 