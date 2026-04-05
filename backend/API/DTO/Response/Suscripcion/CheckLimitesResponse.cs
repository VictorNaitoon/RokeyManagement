namespace API.DTO.Response.Suscripcion
{
    /// <summary>
    /// DTO para el resultado de verificación de límites
    /// </summary>
    public class CheckLimitesResponse
    {
        public bool DentroLimites { get; set; }
        public List<LimiteExcedido> LimitesExcedidos { get; set; } = new List<LimiteExcedido>();

        public class LimiteExcedido
        {
            public string Recurso { get; set; } = string.Empty;
            public int Actual { get; set; }
            public int Limite { get; set; }
            public string Mensaje => $"Ha excedido el límite de {Recurso}. Actual: {Actual}, Límite: {Limite}";
        }
    }
}
