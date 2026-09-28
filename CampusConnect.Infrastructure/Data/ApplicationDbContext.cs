using Microsoft.EntityFrameworkCore;
using CampusConnect.Core.Models;
using CampusConnect.Core.Enums;

namespace CampusConnect.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Recurso> Recursos { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<Evidencia> Evidencias { get; set; }
        public DbSet<Seguimiento> Seguimientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Usuario ---
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Correo).IsUnique();
                entity.Property(u => u.Rol).HasConversion<int>();
            });

            // --- Recurso ---
            modelBuilder.Entity<Recurso>(entity =>
            {
                entity.Property(r => r.Categoria).HasConversion<int>();
            });

            // --- Solicitud ---
            modelBuilder.Entity<Solicitud>(entity =>
            {
                entity.Property(s => s.Estado).HasConversion<int>();
                entity.Property(s => s.Prioridad).HasConversion<int>();

                // Relacion Solicitud -> Estudiante
                entity.HasOne(s => s.Estudiante)
                      .WithMany(u => u.SolicitudesComoEstudiante)
                      .HasForeignKey(s => s.EstudianteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relacion Solicitud -> Responsable (nullable)
                entity.HasOne(s => s.Responsable)
                      .WithMany(u => u.SolicitudesComoResponsable)
                      .HasForeignKey(s => s.ResponsableId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // Relacion Solicitud -> Recurso (nullable)
                entity.HasOne(s => s.Recurso)
                      .WithMany(r => r.Solicitudes)
                      .HasForeignKey(s => s.RecursoId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            // --- Evidencia ---
            modelBuilder.Entity<Evidencia>(entity =>
            {
                entity.HasOne(e => e.Solicitud)
                      .WithMany(s => s.Evidencias)
                      .HasForeignKey(e => e.SolicitudId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Seguimiento ---
            modelBuilder.Entity<Seguimiento>(entity =>
            {
                entity.Property(sg => sg.EstadoAnterior).HasConversion<int?>().IsRequired(false);
                entity.Property(sg => sg.EstadoNuevo).HasConversion<int?>().IsRequired(false);

                entity.HasOne(sg => sg.Solicitud)
                      .WithMany(s => s.Seguimientos)
                      .HasForeignKey(sg => sg.SolicitudId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sg => sg.Usuario)
                      .WithMany(u => u.Seguimientos)
                      .HasForeignKey(sg => sg.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Seed Data ---
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Nombre = "Administrador Sistema",
                    Correo = "admin@campusconnect.edu",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Rol = RolUsuario.Administrativo
                },
                new Usuario
                {
                    Id = 2,
                    Nombre = "Técnico General",
                    Correo = "tecnico@campusconnect.edu",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tecnico@123"),
                    Rol = RolUsuario.ResponsableTecnico
                }
            );
        }
    }
}
