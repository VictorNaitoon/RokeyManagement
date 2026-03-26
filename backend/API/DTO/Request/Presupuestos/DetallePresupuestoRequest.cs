using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request.Presupuestos
{
    public class DetallePresupuestoRequest
    {
        /// <summary>
        /// ID del producto incluido en el presupuesto
        /// </summary>
        [Required(ErrorMessage = "El ID del producto es requerido")]
        public int IdProducto { get; set; }

        /// <summary>
        /// Cantidad del producto
        /// </summary>
        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio pactado (precio negociado para este presupuesto)
        /// </summary>
        [Required(ErrorMessage = "El precio pactado es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio pactado debe ser mayor a 0")]
        public decimal PrecioPactado { get; set; }
    }
}
