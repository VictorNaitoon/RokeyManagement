namespace API.DTO.Request.Productos
{
    public record AjusteStockRequest(int CantidadDelta, string Motivo);
}