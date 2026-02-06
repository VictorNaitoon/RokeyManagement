using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("negocio")]
    public class Negocio
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        [Display(Name = "Nombre o Razón social del Negocio")]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string CUIT { get; set; } = string.Empty;
        [MaxLength(300)]
        public string Direccion { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? LogoURL { get; set; }
        [Required]
        public Enums.EstadoNegocio Estado { get; set; }
        [Required]
        public Enums.TipoNegocio Tipo { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal IngresosBrutos { get; set; }
        public DateOnly FechaInicioActividades { get; set; }
        [MaxLength(100)]
        public string? PuntoDeVenta { get; set; }
        [MaxLength(50)]
        public string? Telefono { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        [MaxLength(100)]
        public string? CondicionVentas { get; set; }

        // Relaciones
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
        public ICollection<Cliente>? Clientes { get; set; }
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    }
}
