using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IPedidoRepository : IRepository<Pedido>
    {
        Task<IEnumerable<Pedido>> GetByClienteIdAsync(int clienteId);
    }
}