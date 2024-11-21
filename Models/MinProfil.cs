using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models
{
    public class MinProfil
    {
        public int BrukerId { get; set; }
        public string Fornavn { get; set; } = string.Empty;
        public string Etternavn { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime OpprettetDato { get; set; }     
        
    }
}

