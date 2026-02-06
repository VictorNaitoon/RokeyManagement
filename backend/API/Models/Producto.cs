using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("producto")]
    public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Identificador del negocio al que pertenece el producto, es obligatorio. Hace refetencia a la tabla "negocio" a través de la clave foránea "IdNegocio".
        /// Al establecer esta relación, se garantiza que cada producto esté vinculado a un negocio existente, lo que facilita la gestión y organización de los productos dentro del sistema.
        /// </summary>
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Identificador del usuario que creó el producto, es obligatorio. Hace referencia a la tabla "usuario" a través de la clave foránea "IdUsuarioCreador".
        /// Al establecer esta relación, se puede rastrear quién fue el responsable de crear cada producto, lo que es útil para fines de auditoría y gestión de usuarios dentro del sistema. 
        /// </summary>
        public int IdUsuarioCreador { get; set; }
        [ForeignKey("IdUsuarioCreador")]
        public Usuario UsuarioCreador { get; set; } = null!;
        /// <summary>
        /// Identificador de la categoría a la que pertenece el producto, es opcional. Hace referencia a la tabla "categoria" a través de la clave foránea "IdCategoria".
        /// Al establecer esta relación, se puede organizar y clasificar los productos en diferentes categorías, lo que facilita la búsqueda y gestión de los productos dentro del sistema.
        /// </summary>
        public int? IdCategoria { get; set; }
        [ForeignKey("IdCategoria")]
        public Categoria? Categoria { get; set; }
        /// <summary>
        /// Es obligatorio. Este campo permite identificar y describir el producto dentro del sistema, y se utiliza en diversas funcionalidades como la búsqueda, la visualización de detalles del producto y la generación de informes.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;
        /// <summary>
        /// Es opcional, se usa para la búsqueda o localización de algún producto en la base de datos.
        /// </summary>
        [MaxLength(50)]
        public string? CodigoBusqueda { get; set; }
        /// <summary>
        /// Es opcional y es para describir al producto. También sirve para búsqueda.
        /// </summary>
        [MaxLength(500)]
        public string? Descripcion { get; set; }
        /// <summary>
        /// Es el precio por el que compra un producto al proveedor. 
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioCompra { get; set; }
        /// <summary>
        /// Es el precio por el que se vende un producto al cliente.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }
        /// <summary>
        /// Es la cantidad actual que hay de ese producto
        /// </summary>
        public int StockActual { get; set; }
        /// <summary>
        /// Es la cantidad mínima que debe haber de ese producto porque sino notifica al usuario la posibilidad de faltante de stock.
        /// </summary>
        public int StockMinimo { get; set; }
        /// <summary>
        /// Es la imagen de un producto y puede ser opcional.
        /// </summary>
        public string? ImagenURL { get; set; }
        /// <summary>
        /// Es para saber si un producto es servicio o no porque por ejemplo en una cerrajería un destrabe de puerta no es un producto pero es un servicio.
        /// </summary>
        public bool EsServicio { get; set; }
        /// <summary>
        /// Es para cuando no tenemos stock de este producto y para no eliminarlo de la base de datos se lo desactiva.
        /// </summary>
        public bool Activo { get; set; } = true;

        // Relaciones
        /// <summary>
        /// El producto puede aparecer en varios detalles de ventas y de compras
        /// </summary>
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
        public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
        /// <summary>
        /// El producto puede aparecer en muchos movimientos de stock que pueden ser ventas, compras u otro motivo.
        /// </summary>
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
        /// <summary>
        /// El producto puede aparecer en muchos carritos
        /// </summary>
        public ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();
        /// <summary>
        /// El producto puede aparecer en muchos detalles de presupuestos.
        /// </summary>
        public ICollection<DetallePresupuesto> DetallesPresupuesto { get; set; } = new List<DetallePresupuesto>();
    }
}
