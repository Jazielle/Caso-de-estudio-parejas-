using System.ComponentModel.DataAnnotations;
using CampusConnect.Core.Enums;

namespace CampusConnect.Core.Models
{
    public class Recurso
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del recurso es requerido.")]
        [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public CategoriaRecurso Categoria { get; set; }

        [Required(ErrorMessage = "El estado es requerido.")]
        [StringLength(100)]
        public string Estado { get; set; } = "Disponible";

        [StringLength(1000)]
        public string? Detalles { get; set; }

        // Navigation properties
        public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
    }
}
