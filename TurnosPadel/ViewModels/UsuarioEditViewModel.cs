using TurnosPadel.Enums;
using System.ComponentModel.DataAnnotations;

namespace TurnosPadel.ViewModels
{
    public class UsuarioEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string numerotel { get; set; }

        public Rol Rol { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string? NuevaPassword { get; set; } // No requerida
    }
}
