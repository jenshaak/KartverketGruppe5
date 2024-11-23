namespace KartverketGruppe5.Services.Interfaces;

public interface IKommunePopulateService
{
    /// <summary>
    /// Populerer databasen med fylker og kommuner fra eksternt API
    /// </summary>
    /// <returns>En statusmelding som beskriver resultatet av operasjonen</returns>
    Task<string> PopulateFylkerOgKommuner();
} 