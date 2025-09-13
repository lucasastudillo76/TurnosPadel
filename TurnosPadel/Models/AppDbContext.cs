using Microsoft.EntityFrameworkCore;
using TurnosPadel.Enums;
using TurnosPadel.Models;

namespace TurnosPadel.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Turno> Turnos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Guardar enum Rol como string en la base
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Rol)
                .HasConversion<string>();

            // Configuración de Turno (opcional, podés expandir después)
            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Usuario)
                .WithMany(u => u.Turnos)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);  // Evita borrado en cascada
        }
    }
}
