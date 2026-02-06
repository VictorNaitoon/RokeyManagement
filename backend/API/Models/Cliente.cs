using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("cliente")]
    public class Cliente
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        [Required]
        [MaxLength(200)]
        [Display(Name = "Nombre o Razón social del Cliente")]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? Apellido { get; set; }
        [MaxLength(50)]
        [Display(Name = "DNI, CUIT o CUIL")]
        public string? Documento { get; set; }
        [MaxLength(50)]
        public string? CondicionIVA { get; set; }
        [MaxLength(50)]
        public string? Telefono { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        [MaxLength(300)]
        public string? Direccion { get; set; }
        public DateOnly FechaAlta { get; set; }

        //Listas
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    }
}

