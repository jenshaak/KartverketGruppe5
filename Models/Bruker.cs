using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KartverketGruppe5.Models
{
    public class Bruker
    {
        [Key]
        public int BrukerId { get; set; }
        
        public string Fornavn { get; set; } = string.Empty;
        
        public string Etternavn { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string Passord { get; set; } = string.Empty;
        
        public string Rolle { get; set; } = "bruker";
        
        public DateTime OpprettetDato { get; set; }
    }
} 