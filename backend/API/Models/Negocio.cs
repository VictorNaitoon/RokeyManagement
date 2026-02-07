using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("negocio")]
    public class Negocio
    {
        /// <summary>
        /// Es el identificador único del negocio, que se genera automáticamente al crear un nuevo negocio en la base de datos.
        /// Se autoincrementa para garantizar que cada negocio tenga un identificador único y no se repita con otros negocios.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es un campo obligatorio que almacena el nombre del negocio o su razón social.
        /// Se utiliza para identificar el sistema, y es importante para la gestión y organización de los datos relacionados con el negocio, como usuarios, productos, ventas, compras, etc.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "Nombre o Razón social del Negocio")]
        public string Nombre { get; set; } = string.Empty;
        /// <summary>
        /// Es un campo obligatorio que almacena el número de CUIT del negocio, que es un identificador fiscal utilizado en Argentina para identificar a las personas jurídicas y físicas que realizan actividades económicas.
        /// Se utiliza para identificar el negocio de manera única y es importante para la gestión fiscal y contable del negocio, así como para cumplir con las obligaciones legales y tributarias correspondientes.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string CUIT { get; set; } = string.Empty;
        /// <summary>
        /// Es un campo obligatorio que almacena la dirección del negocio, que puede incluir la calle, número, ciudad, provincia y código postal.
        /// </summary>
        [MaxLength(300)]
        public string Direccion { get; set; } = string.Empty;
        /// <summary>
        /// Es un campo opcional que almacena la URL del logo del negocio, que es una imagen que representa visualmente al negocio y se utiliza para identificarlo de manera gráfica en el sistema.
        /// Además es para personalizar el sistema con la imagen del negocio, lo que puede mejorar la experiencia del usuario y hacer que el sistema sea más atractivo visualmente.
        /// </summary>
        [MaxLength(500)]
        public string? LogoURL { get; set; }
        /// <summary>
        /// Es un campo obligatorio que almacena el estado del negocio, que puede ser "Activo", "Inactivo" o "Suspendido".
        /// </summary>
        [Required]
        public Enums.EstadoNegocio Estado { get; set; }
        /// <summary>
        /// Es un campo obligatorio que almacena el tipo de negocio, que puede ser "Cerrajería", "Ferretería" o "Ambos", etc.
        /// </summary>
        [Required]
        public Enums.TipoNegocio Tipo { get; set; }
        /// <summary>
        /// Es un campo que tiene el monto total de ingresos brutos del negocio, que se calcula sumando el total de las ventas realizadas por el negocio.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal IngresosBrutos { get; set; }
        /// <summary>
        /// Es un campo que almacena la fecha de inicio de actividades del negocio, que es la fecha en que el negocio comenzó a operar o realizar actividades comerciales.
        /// </summary>
        public DateOnly FechaInicioActividades { get; set; }
        /// <summary>
        /// Es un campo opcional que almacena el punto de venta del negocio, que es un número o código que identifica el lugar físico donde se realizan las ventas, 
        /// y se utiliza para la gestión y organización de las ventas dentro del sistema.
        /// </summary>
        [MaxLength(100)]
        public string? PuntoDeVenta { get; set; }
        /// <summary>
        /// Es un campo opcional que almacena el teléfono de contacto del negocio, que es un número de teléfono que se utiliza para comunicarse con el negocio, 
        /// ya sea para consultas, soporte o cualquier otra razón relacionada con el negocio.
        /// </summary>
        [MaxLength(50)]
        public string? Telefono { get; set; }
        /// <summary>
        /// Es un campo opcional que almacena el correo electrónico de contacto del negocio, que es una dirección de correo electrónico que se utiliza para comunicarse con el negocio,
        /// </summary>
        [MaxLength(100)]
        public string? Email { get; set; }
        /// <summary>
        /// Es un campo opcional que almacena las condiciones de venta del negocio, que son los términos y condiciones bajo los cuales se realizan las ventas,
        /// </summary>
        [MaxLength(100)]
        public string? CondicionVentas { get; set; }

        // Relaciones
        /// <summary>
        /// Es una colección de usuarios asociados al negocio, que representa a las personas que tienen acceso al sistema y pueden realizar acciones dentro del negocio, como administrar productos, realizar ventas, gestionar clientes, etc.
        /// </summary>
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        /// <summary>
        /// Es una colección de productos asociados al negocio, que representa los bienes o servicios que el negocio ofrece a sus clientes para la venta. Cada producto puede tener atributos como nombre, descripción, precio, stock, etc., y se utiliza para gestionar el inventario y las ventas dentro del sistema.
        /// </summary>
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        /// <summary>
        /// Es una colección de categorías asociadas al negocio, que representa las diferentes clasificaciones o agrupaciones de productos dentro del negocio. Cada categoría puede tener un nombre y una descripción, y se utiliza para organizar los productos de manera más eficiente y facilitar la búsqueda y navegación dentro del sistema.
        /// </summary>
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        /// <summary>
        /// Es una colección de proveedores asociados al negocio, que representa a las empresas o personas que suministran los productos o servicios que el negocio ofrece a sus clientes. Cada proveedor puede tener información como nombre, dirección, teléfono, correo electrónico, etc., y se utiliza para gestionar las compras y el inventario dentro del sistema.
        /// </summary>
        public ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
        /// <summary>
        /// Es una colección de clientes asociados al negocio, que representa a las personas o empresas que compran los productos o servicios que el negocio ofrece. Cada cliente puede tener información como nombre, dirección, teléfono, correo electrónico, etc., y se utiliza para gestionar las ventas, el historial de compras y la relación con los clientes dentro del sistema.
        /// </summary>
        public ICollection<Cliente>? Clientes { get; set; }
        /// <summary>
        /// Es una colección de ventas asociadas al negocio, que representa las transacciones comerciales realizadas por el negocio, donde se registran los productos o servicios vendidos, el cliente que realizó la compra, el usuario que realizó la venta, la fecha de la venta, el total de la venta, etc. 
        /// Esta colección es fundamental para llevar un registro de las ventas realizadas por el negocio y para generar informes y análisis relacionados con las ventas.
        /// </summary>
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        /// <summary>
        /// Es una colección de compras asociadas al negocio, que representa las transacciones comerciales realizadas por el negocio para adquirir productos o servicios de los proveedores. En esta colección se registran los productos o servicios comprados, el proveedor que suministró la compra, el usuario que realizó la compra, la fecha de la compra, el total de la compra, etc.
        /// </summary>
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
        /// <summary>
        /// Es una colección de presupuestos asociados al negocio, que representa las propuestas comerciales realizadas por el negocio a sus clientes, donde se registran los productos o servicios incluidos en el presupuesto, el cliente al que se le hizo el presupuesto, el usuario que realizó el presupuesto, la fecha de emisión del presupuesto, la fecha de vencimiento del presupuesto, el estado del presupuesto (pendiente, aceptado o rechazado), el total del presupuesto, etc.
        /// </summary>
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
        /// <summary>
        /// Es una colección de facturas asociadas al negocio, que representa los documentos comerciales que se generan como resultado de las ventas realizadas por el negocio. En esta colección se registran los productos o servicios incluidos en la factura, el cliente al que se le hizo la factura, el usuario que realizó la venta, la fecha de emisión de la factura, el total de la factura, etc.
        /// </summary>
        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    }
}
