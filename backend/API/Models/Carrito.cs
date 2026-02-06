using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("carrito")]
    public class Carrito
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string IdSesion { get; set; } = string.Empty;
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        public int Cantidad { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioAcumulado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
