using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("movimiento_stock")]
    public class MovimientoStock
    {
        /// <summary>
        /// Es el identificador único del movimiento de stock, que se genera automáticamente al crear un nuevo movimiento de stock.
        /// Se autoincrementa para cada nuevo movimiento de stock, lo que garantiza que cada movimiento de stock tenga un identificador único y secuencial dentro de la base de datos.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es para saber a que producto se le hizo el movimiento de stock. Por eso linkea con la tabla producto por medio de IdProducto.
        /// Además, es obligatorio porque un movimiento de stock no puede existir sin estar asociado a un producto específico. Esto garantiza la integridad de los datos 
        /// y permite rastrear correctamente los cambios en el inventario relacionados con cada producto.
        /// </summary>
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        /// <summary>
        /// Es para saber que usuario realizó el movimiento de stock. Por eso linkea con la tabla usuario por medio de IdUsuario.
        /// Además, es obligatorio porque un movimiento de stock no puede existir sin estar asociado a un usuario específico. Esto garantiza la integridad de los datos
        /// </summary>
        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        /// <summary>
        /// Es para saber a que venta y compra se le hizo el movimiento de stock. Por eso linkea con la tabla venta o compra por medio de IdVenta o de IdCompra.
        /// Además, no es obligatorio porque un movimiento de stock puede existir sin estar asociado a una venta específica, como en el caso de movimientos de stock relacionados con compras, ajustes de inventario u otros tipos de movimientos que no están directamente vinculados a una venta. Esto permite una mayor flexibilidad en la gestión del inventario y la capacidad de registrar diferentes tipos de movimientos de stock según sea necesario.
        /// </summary>
        public int? IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public virtual Venta? Venta { get; set; }
        public int? IdCompra { get; set; }
        [ForeignKey("IdCompra")]
        public virtual Compra? Compra { get; set; }
        /// <summary>
        /// Es para saber en que fecha se realizó el movimiento de stock. Esto es importante para llevar un registro cronológico de los cambios en el inventario y para poder analizar las tendencias de stock a lo largo del tiempo. 
        /// Además, es útil para fines de auditoría y para identificar posibles problemas o irregularidades en la gestión del inventario.
        /// </summary>
        public DateTime FechaMovimiento { get; set; }
        /// <summary>
        /// Es para saber la cantidad de productos que se movieron en el stock. Esto es fundamental para llevar un control preciso del inventario y para calcular el stock actual de cada producto.
        /// </summary>
        public int Cantidad { get; set; }
        /// <summary>
        /// Es para saber el tipo de movimiento de stock que se realizó, ya sea una entrada (compra) o una salida (venta). Esto es esencial para entender el impacto del movimiento en el inventario y para realizar análisis y reportes relacionados con la gestión del stock.
        /// </summary>
        public Enums.TipoMovimiento TipoMovimiento { get; set; }
    }
}
