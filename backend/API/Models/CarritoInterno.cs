using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("carrito_interno")]
    public class CarritoInterno
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IdNegocio { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public Enums.EstadoCarritoInterno Estado { get; set; } = Enums.EstadoCarritoInterno.Activo;

        [MaxLength(100)]
        public string? Nombre { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Relaciones
        [ForeignKey(nameof(IdNegocio))]
        public Negocio? Negocio { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }

        public ICollection<CarritoInternoItem> Items { get; set; } = new List<CarritoInternoItem>();
    }
}
