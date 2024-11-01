using Dapper;
using MySqlConnector;
using System;
using System.Data;

namespace KartverketGruppe5.Data
{
    // Tjenesten lar deg utføre CRUD-operasjoner (Create, Read, Update, Delete) på GeoChanges tabell
    public class GeoChangeService
    {
        // Definerer en variabel av type IConfigration som brukes til å hente "connection string" fra "appsettings.json"
        private readonly IConfiguration _configuration;

        // Definerer en variabel som beholder "connection string" som hentes fra "appsettings.json" under nøkkelen "DefaultConnection"
        private readonly string? _connectionString;

        public GeoChangeService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Get MariaDB Connection
        // Definerer en property som returnerer en ny instans av en "database connection" (MySqlConnection) ved å bruke "connection string".
        private IDbConnection Connection => new MySqlConnection(_connectionString);

        // Setter inn en ny rad i GeoChanges tabell
        public void AddGeoChange(string description, string geoJson)
        {
            // Denne konstruksjonen brukes for å sikre at ressursen, i dette tilfellet "dbConnection":
            // blir korrekt håndtert OG frigjort etter bruk. 
            using (IDbConnection dbConnection = Connection)
            {
                // Dapper bruker parameteriserte verdier for å unngå SQL-injeksjon.
                // Verdiene @Description og @GeoJson blir fylt med parameterne fra metoden.
                string query = @"INSERT INTO GeoChanges (Description, GeoJson) 
                             VALUES (@Description, @GeoJson)";
                dbConnection.Execute(query, new { Description = description, GeoJson = geoJson });
            } 
        }

        // Henter alle rader fra GeoChanges tabellen.
        public IEnumerable<GeoChange> GetAllGeoChanges()
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = @"SELECT * FROM GeoChanges";
                return dbConnection.Query<GeoChange>(query);
            }
        }

        // Returnerer én enkelt GeoChange basert på dens unike Id
        public GeoChange GetGeoChangeById(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = "SELECT * FROM GeoChanges WHERE Id = @Id";

                // QuerySingleOrDefault returnerer én rad, eller null hvis ingen match finnes.
                // Dette er nyttig for operasjoner som oppdatering eller sletting, hvor man typisk vil hente en spesifikk rad.
                return dbConnection.QuerySingleOrDefault<GeoChange>(query, new { Id = id });
            }
        }

        //  Oppdaterer en eksisterende GeoChange rad i databasen basert på Id
        public void UpdateGeoChange(int id, string description, string geoJsonData)
        {
            using (IDbConnection dbConnection = Connection)
            {
                // Oppdaterer verdiene i Description og GeoJson kolonnene for raden som har den spesifikke Id.
                string query = @"UPDATE GeoChanges 
                             SET Description = @Description, GeoJson = @GeoJson 
                             WHERE Id = @Id";
                dbConnection.Execute(query, new { Id = id, Description = description, GeoJson = geoJsonData });
            }
        }

        // Sletter en eksisterende GeoChange rad basert på dens Id
        public void DeleteGeoChange(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                // Fjerner raden med gitt Id fra GeoChanges tabellen
                string query = "DELETE FROM GeoChanges WHERE Id = @Id";
                dbConnection.Execute(query, new { Id = id });
            }
        }
    }
}