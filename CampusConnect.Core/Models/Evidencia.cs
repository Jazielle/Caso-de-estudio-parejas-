using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusConnect.Core.Models
{
    public class Evidencia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SolicitudId { get; set; }
        [ForeignKey("SolicitudId")]
        public Solicitud? Solicitud { get; set; }

        [Required(ErrorMessage = "La ruta del archivo es requerida.")]
        [StringLength(500)]
        public string RutaArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TipoContenido { get; set; } = string.Empty;

        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
    }
}
