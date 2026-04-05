using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTO.Request.Presupuestos
{
    public class UpdatePresupuestoRequest
    {
        /// <summary>
        /// Nuevo estado del presupuesto
        /// </summary>
        [Required(ErrorMessage = "El estado es requerido")]
        public Enums.EstadoPresupuesto Estado { get; set; }
    }
}
