using BicTechBack.src.Core.DTOs;

namespace BicTechBack.src.Core.Interfaces
{
    public interface ICarritoService
    {
        Task<IEnumerable<CarritoDTO>> GetAllCarritosAsync();
        Task<CarritoDTO> GetCarritoByUsuarioIdAsync(int usuarioId);
        Task<CarritoDTO> AddProductoToCarritoAsync(int usuarioId, int productoId, int cantidad);
        Task<CarritoDTO> UpdateAmountProductoAsync(int usuarioId, int productoId, int cantidad);
        Task<CarritoDTO> DeleteProductoFromCarritoAsync(int usuarioId, int productoId);
        Task<CarritoDTO> ClearCarritoAsync(int usuarioId);
        Task<(IEnumerable<CarritoDTO> Carritos, int Total)> GetCarritosAsync(int page, int pageSize, string? filtro);

    }
}
