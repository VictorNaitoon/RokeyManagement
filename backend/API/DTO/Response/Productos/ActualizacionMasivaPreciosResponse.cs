namespace API.DTO.Response.Productos
{
    public class DetalleActualizacion
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal PrecioVentaAnterior { get; set; }
        public decimal PrecioVentaNuevo { get; set; }
        public decimal PrecioCompraAnterior { get; set; }
        public decimal PrecioCompraNuevo { get; set; }
    }

    public class ActualizacionMasivaPreciosResponse
    {
        public int TotalProductosActualizados { get; set; }
        public int TotalProductosVerificados { get; set; }
        public List<DetalleActualizacion> Detalles { get; set; } = new List<DetalleActualizacion>();
        public DateTime FechaActualizacion { get; set; }
    }
}
