using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IPedidoDetalleRepository
    {
        Task<IEnumerable<PedidoDetalle>> GetAllAsync();
        Task<PedidoDetalle?> GetByIdAsync(int id);
        Task<PedidoDetalle> AddAsync(PedidoDetalle pedidoDetalle);
        Task<PedidoDetalle> UpdateAsync(PedidoDetalle pedidoDetalle);
        Task<bool> DeleteAsync(int id);
    }
}
