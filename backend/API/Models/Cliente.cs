using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("cliente")]
    public class Cliente
    {
        /// <summary>
        /// Es la clave primaria que se genera automáticamente al crear un nuevo cliente en la base de datos. 
        /// Se autoincrementa para garantizar que cada cliente tenga un identificador único y no se repita con otros clientes.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es para saber a qué negocio pertenece el cliente, linkeando la tabla "negocio" por IdNegocio.
        /// Si agregamos un cliente al sistema, es importante saber a qué negocio pertenece ese cliente.
        /// </summary>
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Es para saber el nombre o razón social del cliente, que es un campo obligatorio y se utiliza para identificar al cliente de manera única en el sistema.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "Nombre o Razón social del Cliente")]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? Apellido { get; set; }
        /// <summary>
        /// Es para saber el número de documento del cliente, que puede ser un DNI, CUIT o CUIL, dependiendo del tipo de cliente y su situación fiscal.
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "DNI, CUIT o CUIL")]
        public string? Documento { get; set; }
        /// <summary>
        /// Es para saber la condición frente al IVA del cliente, que puede ser "Responsable Inscripto", "Monotributista", "Exento", etc., dependiendo de su situación fiscal.
        /// </summary>
        [MaxLength(50)]
        public string? CondicionIVA { get; set; }
        /// <summary>
        /// Es para saber el número de teléfono y email del cliente, que es un campo opcional pero puede ser útil para contactar al cliente o enviarle notificaciones relacionadas con sus compras, presupuestos, etc.
        /// </summary>
        [MaxLength(50)]
        public string? Telefono { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        /// <summary>
        /// Es para saber la dirección del cliente, que es un campo opcional pero puede ser útil para enviarle productos o realizar entregas a domicilio, así como para tener un registro completo de la información del cliente en el sistema.
        /// </summary>
        [MaxLength(300)]
        public string? Direccion { get; set; }
        /// <summary>
        /// Es para saber la fecha de alta del cliente, que es la fecha en que el cliente fue registrado o agregado al sistema.
        /// </summary>
        public DateTime FechaAlta { get; set; }

        //Listas
        /// <summary>
        /// Es el listado de las ventas realizadas por el cliente, es decir, las compras que el cliente ha hecho en el negocio.
        /// </summary>
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        /// <summary>
        /// Es el listado de los presupuestos realizados para el cliente, es decir, los presupuestos que se le han hecho al cliente para mostrarle una propuesta de productos o servicios con sus precios correspondientes.
        /// </summary>
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    }
}

