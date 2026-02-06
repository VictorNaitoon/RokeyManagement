using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("pago")]
    public class Pago
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public Venta Venta { get; set; } = null!;
        public Enums.MetodoPago MetodoPago { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }
    }
}
