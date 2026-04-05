using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTO.Request.CarritoInterno
{
    public class ConvertirCarritoRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdCaja { get; set; }

        [Required]
        public Enums.MetodoPago FormaPago { get; set; }

        public int? IdCliente { get; set; }
    }
}
