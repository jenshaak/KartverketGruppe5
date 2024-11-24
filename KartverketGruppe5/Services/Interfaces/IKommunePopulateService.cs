namespace KartverketGruppe5.Services.Interfaces;

public interface IKommunePopulateService
{
    /// <summary>
    /// Populerer databasen med fylker og kommuner fra eksternt API
    /// </summary>
    /// <returns>En statusmelding som beskriver resultatet av operasjonen</returns>
    Task<string> PopulateFylkerOgKommuner();

    /// <summary>
    /// Henter fylkesnavn basert på kommunenummer
    /// </summary>
    /// <param name="kommunenummer">Kommunenummer hvor de første to sifrene indikerer fylket</param>
    /// <returns>Navnet på fylket</returns>
    /// <exception cref="ArgumentException">Kastes hvis kommunenummeret er ugyldig eller fylket er ukjent</exception>
    string GetFylkesnavnFromKommunenummer(string kommunenummer);
} 