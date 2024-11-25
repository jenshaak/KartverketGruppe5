using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models.ViewModels
{
    /// <summary>
    /// ViewModel for innlogging
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email er påkrevd")]
        [EmailAddress(ErrorMessage = "Ugyldig email-adresse")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passord er påkrevd")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
} 