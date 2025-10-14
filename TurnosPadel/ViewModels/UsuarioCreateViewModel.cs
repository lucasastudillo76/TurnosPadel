using System.ComponentModel.DataAnnotations;
using TurnosPadel.Enums;

namespace TurnosPadel.ViewModels
{
    public class UsuarioCreateViewModel
    {
        [Required]
        public string NombreCompleto { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string numerotel { get; set; }

        [Required]
        public Rol Rol { get; set; }
    }
}
