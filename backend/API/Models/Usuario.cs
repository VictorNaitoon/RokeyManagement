using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public Negocio Negocio { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public Enums.RolUsuario Rol { get; set; }
        public bool Activo { get; set; } = true;

        //Propiedades y relaciones
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
    }
}
