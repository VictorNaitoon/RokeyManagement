namespace API.DTO.Response.Caja
{
    public class CajaResponse
    {
        public int Id { get; set; }
        public int Id_negocio { get; set; }
        public int Id_usuario_apertura { get; set; }
        public int? Id_usuario_cierre { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal? MontoFinal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class EstadoCajaResponse
    {
        public bool TieneCajaAbierta { get; set; }
        public CajaResponse? Caja { get; set; }
    }
}
