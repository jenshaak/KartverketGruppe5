using KartverketGruppe5.Models;
using MySqlConnector;



namespace KartverketGruppe5.Services
{
    public class MinProfilService
    {
        private readonly string _connectionString;

        public MinProfilService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Bruker? GetBrukerByEmail(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                const string query = "SELECT * FROM Bruker WHERE Email = @Email";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Bruker
                        {
                            BrukerId = Convert.ToInt32(reader["BrukerId"]),
                            Fornavn = reader["Fornavn"].ToString() ?? string.Empty,
                            Etternavn = reader["Etternavn"].ToString() ?? string.Empty,
                            Email = reader["Email"].ToString() ?? string.Empty,
                            OpprettetDato = Convert.ToDateTime(reader["OpprettetDato"])
                        };

                    }
                }
            }

            return null;
        }
    }
}

