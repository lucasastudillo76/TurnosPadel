using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;  // <-- Necesario para [NotMapped]
using TurnosPadel.Enums;

namespace TurnosPadel.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string numerotel { get; set; }

        [Required]
        public string ContraseñaHash { get; set; }

        [Required]
        public Rol Rol { get; set; }  // Enum Rol (Admin o Socio)

        public ICollection<Turno> Turnos { get; set; } = new List<Turno>();

        [NotMapped]  // Esto evita que se cree una columna en la base de datos para esta propiedad
        [DataType(DataType.Password)]
        public string Password { get; set; }  // Para recibir la contraseña en texto plano en el registro
    }
}
