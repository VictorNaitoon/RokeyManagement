using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTO.Request.Ventas
{
    public class PagoRequest
    {
        [Required]
        public Enums.MetodoPago MetodoPago { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }
    }
}
