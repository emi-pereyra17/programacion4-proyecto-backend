using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Interfaces
{
    public interface ICarritoRepository
    {
        Task<IEnumerable<Carrito>> GetAllAsync();
        Task<Carrito?> GetByUsuarioIdAsync(int usuarioId);
        Task<Carrito> AddProductoAsync(int usuarioId, int productoId, int cantidad);
        Task<Carrito> UpdateAsync(int usuarioId, int productoId, int cantidad);
        Task<Carrito> DeleteAsync(int usuarioId, int productoId);
        Task<Carrito> ClearAsync(int usuarioId);
    }
}
