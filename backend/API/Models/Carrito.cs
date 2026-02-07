using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("carrito")]
    public class Carrito
    {
        /// <summary>
        /// Identificador único del carrito. Se genera automaticamente al crear un nuevo carrito.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Identificador de la sesión del usuario. Es obligatorio y se utiliza para asociar el carrito con la sesión del usuario que lo creó. 
        /// Esto permite que el sistema pueda identificar y gestionar el carrito de manera adecuada, incluso si el usuario no ha iniciado sesión o no tiene una cuenta en el sistema. 
        /// Es una cookie que se genera al crear un nuevo carrito y se almacena en el navegador del usuario para mantener la persistencia del carrito durante la sesión.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string IdSesion { get; set; } = string.Empty;
        /// <summary>
        /// Identificador del producto que se ha agregado al carrito. Es obligatorio y hace referencia a la tabla "producto" a través de la clave foránea "IdProducto".
        /// Permite agregar productos al carrito y realizar un seguimiento de los productos que el usuario cliente seleccionó para su compra.
        /// </summary>
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        /// <summary>
        /// Cantidad del producto que se ha agregado al carrito. Es obligatorio y se utiliza para indicar la cantidad de unidades del producto que el usuario cliente desea comprar.
        /// </summary>
        [Required]
        public int Cantidad { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        /// <summary>
        /// Es el precio acumulado del producto en el carrito, calculado como el precio de venta del producto multiplicado por la cantidad agregada al carrito. Es obligatorio y se utiliza para mostrar el costo total de los productos en el carrito al usuario cliente.
        /// </summary>
        [Required]
        public decimal PrecioAcumulado { get; set; }
        /// <summary>
        /// Fecha y hora en que se creó el carrito. Es obligatorio y se utiliza para registrar el momento en que el usuario cliente agregó productos al carrito. Esto puede ser útil para fines de seguimiento, análisis de comportamiento del usuario y gestión de carritos abandonados.
        /// </summary>
        [Required]
        public DateTime FechaCreacion { get; set; }
    }
}
