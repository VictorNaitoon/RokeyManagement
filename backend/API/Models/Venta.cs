using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("venta")]
    public class Venta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es para saber a que negocio pertenece la venta. Por eso linkea con la tabla negocio por medio de IdNegocio.
        /// </summary>
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Es para saber que usuario realizó la venta. Por eso linkea con la tabla usuario por medio de IdUsuario.
        /// </summary>
        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        /// <summary>
        /// Es para saber a que cliente te compro productos. 
        /// Por eso linkea con la tabla negocio por medio de IdCliente.
        /// Esto no es requerido porque puede ser una venta a consumidor final.
        /// </summary>
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }
        /// <summary>
        /// Es para saber en que fecha se realizó la venta. 
        /// </summary>
        public DateTime FechaVenta { get; set; }
        /// <summary>
        /// Es para saber cuanto es el valor total de la venta.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalVenta { get; set; }

        //Relaciones y listas
        /// <summary>
        /// Es el listado de los detalles de la venta, es decir, los productos que se vendieron en esta venta.
        /// </summary>
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
        /// <summary>
        /// Es el listado de los movimientos de stock que se generaron por esta venta.
        /// </summary>
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
        /// <summary>
        /// Es el listado de las facturas asociadas a esta venta.
        /// </summary>
        public ICollection<Factura>? Facturas { get; set; }
        /// <summary>
        /// Es el listado de los pagos realizados para esta venta que pueden ser transferencia, efectivo, etc.
        /// </summary>
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
