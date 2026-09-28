using System.ComponentModel.DataAnnotations;
using CampusConnect.Core.Enums;

namespace CampusConnect.Core.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(200)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public RolUsuario Rol { get; set; }

        // Navigation properties
        public ICollection<Solicitud> SolicitudesComoEstudiante { get; set; } = new List<Solicitud>();
        public ICollection<Solicitud> SolicitudesComoResponsable { get; set; } = new List<Solicitud>();
        public ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();
    }
}
