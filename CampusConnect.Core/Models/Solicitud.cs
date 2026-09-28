using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusConnect.Core.Enums;

namespace CampusConnect.Core.Models
{
    public class Solicitud
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es requerido.")]
        [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida.")]
        [StringLength(2000)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

        [Required]
        public PrioridadSolicitud Prioridad { get; set; } = PrioridadSolicitud.Media;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // FK Estudiante
        [Required]
        public int EstudianteId { get; set; }
        [ForeignKey("EstudianteId")]
        public Usuario? Estudiante { get; set; }

        // FK Responsable (nullable)
        public int? ResponsableId { get; set; }
        [ForeignKey("ResponsableId")]
        public Usuario? Responsable { get; set; }

        // FK Recurso (nullable)
        public int? RecursoId { get; set; }
        [ForeignKey("RecursoId")]
        public Recurso? Recurso { get; set; }

        // Navigation properties
        public ICollection<Evidencia> Evidencias { get; set; } = new List<Evidencia>();
        public ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();
    }
}
