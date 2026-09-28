using System.ComponentModel.DataAnnotations;
using CampusConnect.Core.Enums;

namespace CampusConnect.Core.DTOs
{
    public class CrearSolicitudDto
    {
        [Required(ErrorMessage = "El título es requerido.")]
        [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida.")]
        [StringLength(2000)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public PrioridadSolicitud Prioridad { get; set; } = PrioridadSolicitud.Media;

        public int? RecursoId { get; set; }
    }

    public class ActualizarSolicitudDto
    {
        [Required]
        public EstadoSolicitud Estado { get; set; }

        public int? ResponsableId { get; set; }

        [Required(ErrorMessage = "El comentario es requerido al actualizar.")]
        [StringLength(2000)]
        public string Comentario { get; set; } = string.Empty;
    }

    public class SolicitudResponseDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public string EstudianteNombre { get; set; } = string.Empty;
        public string? ResponsableNombre { get; set; }
        public string? RecursoNombre { get; set; }
        public List<EvidenciaDto> Evidencias { get; set; } = new();
        public List<SeguimientoDto> Seguimientos { get; set; } = new();
    }

    public class EvidenciaDto
    {
        public int Id { get; set; }
        public string RutaArchivo { get; set; } = string.Empty;
        public string TipoContenido { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
    }

    public class SeguimientoDto
    {
        public int Id { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public string? EstadoAnterior { get; set; }
        public string? EstadoNuevo { get; set; }
        public string AutorNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
