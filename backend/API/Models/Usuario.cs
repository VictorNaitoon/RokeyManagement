using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es para saber a que negocio pertenece el usuario. 
        /// Por eso linkea con la tabla negocio por medio de IdNegocio.
        /// </summary>
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Es para saber el nombre del usuario
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        /// <summary>
        /// Es para saber el apellido del usuario
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;
        /// <summary>
        /// Es para saber el email del usuario, que es único y se utiliza para iniciar sesión.
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Es para saber la contraseña del usuario, que se almacena como un hash por seguridad.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        /// <summary>
        /// Es para saber el rol del usuario dentro del negocio, 
        /// que puede ser Administrador, Empleado o Contador.
        /// </summary>
        [Required]
        public Enums.RolUsuario Rol { get; set; }
        /// <summary>
        /// Es para saber si el usuario está activo o inactivo.
        /// Es más que nada para no eliminar definitivamente a un usuario, sino para desactivarlo y que no pueda iniciar sesión ni realizar acciones en el sistema.
        /// </summary>
        public bool Activo { get; set; } = true;

        //Propiedades y relaciones
        /// <summary>
        /// Es el listado de los presupuestos que creó el usuario, 
        /// es decir, los presupuestos que se generaron por este usuario.
        /// </summary>
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
        /// <summary>
        /// Es el listado de las categorías que creó el usuario.
        /// </summary>
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        /// <summary>
        /// Es el listado de las ventas que realizó el usuario, guardando así un historial de las ventas realizadas por cada usuario dentro del negocio.
        /// </summary>
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        /// <summary>
        /// Es el listado de las compras que realizó el usuario, guardando así un historial de las compras realizadas por cada usuario dentro del negocio.
        /// </summary>
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
        /// <summary>
        /// Es el listado de los productos que creó el usuario, guardando así un historial de los productos creados y modificados por cada usuario dentro del negocio.
        /// </summary>
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        /// <summary>
        /// Es el listado de los movimientos de stock que se generaron por las acciones del usuario, como ventas, compras o ajustes de stock, guardando así un historial de los movimientos de stock realizados por cada usuario dentro del negocio.
        /// </summary>
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
    }
}
