using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("compra")]
    public class Compra
    {
        /// <summary>
        /// Identificador único de la compra. Se genera automáticamente al crear una nueva compra en la base de datos. Se autoincrementa con cada nuevo registro, lo que garantiza que cada compra tenga un identificador único y secuencial dentro de la base de datos. Esto es fundamental para la gestión y organización de las compras dentro del sistema.
        /// Se autoincrementa.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Identificador del negocio al que pertenece la compra. Es obligatorio porque cada compra debe estar asociada a un negocio específico. Hace referencia a la tabla "negocio" a través de la clave foránea "IdNegocio". 
        /// Esto permite organizar las compras por negocio y facilita la gestión de las compras dentro del sistema lo que es esencial para el seguimiento y control de las compras realizadas por cada negocio.
        /// </summary>
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Identificador del proveedor al que se le realizó la compra. Es obligatorio porque cada compra debe estar asociada a un proveedor específico. Hace referencia a la tabla "proveedor" a través de la clave foránea "IdProveedor".
        /// </summary>
        [Required]
        public int IdProveedor { get; set; }
        [ForeignKey("IdProveedor")]
        public virtual Proveedor Proveedor { get; set; } = null!;
        /// <summary>
        /// Identificador del usuario que realizó la compra. Es obligatorio porque cada compra debe estar asociada a un usuario específico. 
        /// Hace referencia a la tabla "usuario" a través de la clave foránea "IdUsuario".
        /// Permite hacer un seguimiento de quién realizó cada compra, lo que es importante para la gestión y control de las compras dentro del sistema, así como para fines de auditoría y análisis de las actividades de compra realizadas por los usuarios.
        /// </summary>
        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        /// <summary>
        /// Es la fecha en que se realizó la compra. Es importante para llevar un registro cronológico de las compras realizadas, lo que facilita el seguimiento y análisis de las actividades de compra dentro del sistema. Además, es útil para fines de auditoría y para identificar posibles problemas o irregularidades en la gestión de las compras.
        /// </summary>
        public DateTime FechaCompra { get; set; }
        /// <summary>
        /// Es el monto total de la compra, se calcula sumando el total de cada detalle de compra asociado a la compra.
        /// Importante para llevar un control preciso de los gastos y generar informes financieros de las compras realizadas.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGasto { get; set; }
        /// <summary>
        /// Es opcional y almacena el número de comprobante o factura asociada a la compra.
        /// </summary>
        [MaxLength(50)]
        public string? NumeroComprobante { get; set; }

        // Listas
        /// <summary>
        /// Una compra puede tener múltiples detalles de compra asociados. 
        /// </summary>
        public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
        /// <summary>
        /// Una compra puede generar muchos movimientos en el stock, ya que cada detalle de compra puede afectar el inventario de diferentes productos.
        /// </summary>
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
    }
}
