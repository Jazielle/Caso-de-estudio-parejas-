using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusConnect.Core.Enums;

namespace CampusConnect.Core.Models
{
    public class Seguimiento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SolicitudId { get; set; }
        [ForeignKey("SolicitudId")]
        public Solicitud? Solicitud { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "El comentario es requerido.")]
        [StringLength(2000)]
        public string Comentario { get; set; } = string.Empty;

        public EstadoSolicitud? EstadoAnterior { get; set; }
        public EstadoSolicitud? EstadoNuevo { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
