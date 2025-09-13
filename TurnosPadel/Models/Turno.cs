using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.ComponentModel.DataAnnotations;

namespace TurnosPadel.Models
{
    public class Turno
    {
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; }  // Fecha del turno

        [Required]
        public TimeSpan HoraInicio { get; set; }  // Ej: 14:00

        [Required]
        public TimeSpan Duracion { get; set; } = TimeSpan.FromMinutes(90);  // Duración por defecto 1:30

        [Required]
        public int? Cancha { get; set; }

        [Required]
        public bool EsFijo { get; set; }  // Si es un turno recurrente o fijo

        // FK y navegación
        [Required]
        public int? UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }
}
