using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("detalle_venta")]
    public class DetalleVenta
    {
        /// <summary>
        /// Es el identificador único para cada detalle de venta, es obligatorio. Se genera automáticamente.
        /// Se autoincrementa con cada nuevo detalle de venta que se agrega a la base de datos, lo que garantiza que cada registro tenga un valor único y secuencial. 
        /// Esto facilita la gestión y referencia de los detalles de venta dentro del sistema, permitiendo una identificación clara y eficiente de cada registro en la tabla "detalle_venta".
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es el identificador de la venta a la que pertenece este detalle de venta, es obligatorio. Hace referencia a la tabla "venta" a través de la clave foránea "IdVenta".
        /// </summary>
        [Required]
        public int IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public virtual Venta Venta { get; set; } = null!;
        /// <summary>
        /// Es el identificador del producto que se vendió, es obligatorio. Hace referencia a la tabla "producto" a través de la clave foránea "IdProducto".
        /// Indica qué producto específico se vendió en esta transacción, lo que permite rastrear y gestionar el inventario de productos dentro del sistema.
        /// </summary>
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        /// <summary>
        /// Es la cantidad de unidades del producto que se vendieron en esta transacción, es obligatorio. Este campo es esencial para calcular el total de la venta y para gestionar el inventario de productos dentro del sistema.
        /// </summary>
        public int Cantidad { get; set; }
        /// <summary>
        /// Es el precio unitario al que se vendió el producto en esta transacción, es obligatorio. Este campo es crucial para calcular el total de la venta y para realizar análisis de precios dentro del sistema.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}
