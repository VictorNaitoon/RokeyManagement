using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request.Ventas
{
    public class CrearVentaRequest
    {
        public int? IdCliente { get; set; }

        [Required(ErrorMessage = "La venta debe tener al menos un producto")]
        public List<DetalleVentaRequest> Detalles { get; set; } = new();

        [Required(ErrorMessage = "Debe registrar al menos un método de pago")]
        public List<PagoRequest> Pagos { get; set; } = new();
    }
}
