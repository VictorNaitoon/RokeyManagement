using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("detalle_compra")]
    public class DetalleCompra
    {
        /// <summary>
        /// Identificador único del detalle de compra, es obligatorio. Se genera automáticamente al agregar un nuevo detalle de compra a la base de datos.
        /// Se autoincrementa con cada nuevo registro, sirve para la gestión y organización de los detalles de compra dentro del sistema.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Identificador de la compra a la que pertenece el detalle de compra, es obligatorio. Hace referencia a la tabla "compra" a través de la clave foránea "IdCompra".
        /// </summary>
        [Required]
        public int IdCompra { get; set; }
        [ForeignKey("IdCompra")]
        public virtual Compra Compra { get; set; } = null!;
        /// <summary>
        /// Identificador del producto que se está comprando, es obligatorio. Hace referencia a la tabla "producto" a través de la clave foránea "IdProducto".
        /// Ayuda a identificar qué producto se está comprando en cada detalle de compra, lo que es esencial para el seguimiento de inventario, la gestión de compras y la generación de informes dentro del sistema.
        /// </summary>
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        /// <summary>
        /// Cantidad del producto que se está comprando. Es escencial para calcular el total de la compra, gestionar el inventario y generar informes precisos sobre las compras realizadas dentro del sistema.
        /// </summary>
        public int Cantidad { get; set; }
        /// <summary>
        /// Es el precio unitario de la compra del producto. Ayuda a calcular el total de la compra.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}
