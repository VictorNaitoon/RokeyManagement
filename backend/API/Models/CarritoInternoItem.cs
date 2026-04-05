using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("carrito_interno_item")]
    public class CarritoInternoItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CarritoInternoId { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [MaxLength(500)]
        public string? Notas { get; set; }

        // Relaciones
        [ForeignKey(nameof(CarritoInternoId))]
        public CarritoInterno? CarritoInterno { get; set; }

        [ForeignKey(nameof(IdProducto))]
        public Producto? Producto { get; set; }
    }
}
