using API.Models;

namespace API.DTO.Response.Ventas
{
    public class PagoResponse
    {
        public int Id { get; set; }
        public Enums.MetodoPago MetodoPago { get; set; }
        public string? MetodoPagoNombre { get; set; }
        public decimal Monto { get; set; }
    }
}
