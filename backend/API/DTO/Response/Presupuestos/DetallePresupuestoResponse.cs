namespace API.DTO.Response.Presupuestos
{
    public class DetallePresupuestoResponse
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioPactado { get; set; }
        public decimal Subtotal => Cantidad * PrecioPactado;
    }
}
