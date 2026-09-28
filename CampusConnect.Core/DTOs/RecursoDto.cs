using System.ComponentModel.DataAnnotations;
using CampusConnect.Core.Enums;

namespace CampusConnect.Core.DTOs
{
    public class CrearRecursoDto
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public CategoriaRecurso Categoria { get; set; }

        [Required]
        [StringLength(100)]
        public string Estado { get; set; } = "Disponible";

        [StringLength(1000)]
        public string? Detalles { get; set; }
    }

    public class RecursoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Detalles { get; set; }
        public int TotalSolicitudes { get; set; }
    }

    public class ReporteKpiDto
    {
        public int TotalRecibidas { get; set; }
        public int TotalPendientes { get; set; }
        public int TotalCerradas { get; set; }
        public double TiempoPromedioAtencionHoras { get; set; }
        public List<ResponsableKpiDto> ResponsablesAsignados { get; set; } = new();
    }

    public class ResponsableKpiDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int SolicitudesAsignadas { get; set; }
        public int SolicitudesCerradas { get; set; }
    }
}
