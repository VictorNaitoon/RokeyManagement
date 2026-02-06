using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("pago")]
    public class Pago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        /// <summary>
        /// Es para saber a que venta pertenece el pago. Por eso linkea con la tabla venta por medio de IdVenta.
        /// Se requiere porque cada pago debe estar asociado a una venta, aunque una venta puede tener varios pagos (por ejemplo, si el cliente paga con tarjeta y efectivo).
        /// </summary>
        public int IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public Venta Venta { get; set; } = null!;
        /// <summary>
        /// Es para saber que método de pago se utilizó en esta venta. Por ejemplo, si el cliente pagó con efectivo, tarjeta de crédito, transferencia bancaria, etc.
        /// </summary>
        public Enums.MetodoPago MetodoPago { get; set; }
        /// <summary>
        /// Es para saber cuanto se pagó en esta venta. Esto es importante porque una venta puede tener varios pagos, por ejemplo, si el cliente pagó con tarjeta y efectivo, entonces cada pago tendrá un monto diferente.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }
    }
}
