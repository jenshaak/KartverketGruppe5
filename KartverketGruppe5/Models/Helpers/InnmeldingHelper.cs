namespace KartverketGruppe5.Models.Helpers
{
    /// <summary>
    /// Hjelpemetode for innmeldinger
    /// </summary>
    public static class InnmeldingHelper
    {
        public static List<string> GetAllStatuses()
        {
            return new List<string>
            {
                "Ny",
                "Under behandling",
                "Godkjent",
                "Avvist"
            };
        }

        public static string GetStatusClass(string status) => status switch
        {
            "Ny" => "bg-blue-100 text-blue-800",
            "Under behandling" => "bg-yellow-100 text-yellow-800",
            "Godkjent" => "bg-green-100 text-green-800",
            "Avvist" => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }
} 