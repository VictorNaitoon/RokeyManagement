using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request.Presupuestos
{
    public class CreatePresupuestoRequest
    {
        /// <summary>
        /// ID del cliente opcional. Si es null, se asignará "Consumidor Final"
        /// </summary>
        public int? IdCliente { get; set; }

        /// <summary>
        /// Fecha de vencimiento del presupuesto. Si no se especifica, se usará 30 días por defecto
        /// </summary>
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>
        /// Lista de detalles del presupuesto (productos/servicios)
        /// </summary>
        [Required(ErrorMessage = "El presupuesto debe tener al menos un producto")]
        public List<DetallePresupuestoRequest> Detalles { get; set; } = new();
    }
}
