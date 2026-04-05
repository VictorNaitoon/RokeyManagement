using API.Models;

namespace API.Repository
{
    public interface IVentaRepository
    {
        Task<Venta> CreateVenta(Venta venta);
        Task UpdateVenta(Venta venta);
        Task<Venta> GetVentaById(int id);
        Task<List<Venta>> GetVentas();
        Task AnularVenta(int id);
        Task AperturaCaja(int idNegocio, int idUsuario);
        Task CierreCaja(int idNegocio, int idUsuario);
        Task<List<Venta>> GetVentasByUsuario(int idUsuario);
        Task<List<Venta>> GetVentasByDate(DateTime date);
    }
}
