using BicTechBack.src.Core.DTOs;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoDTO>> GetAllProductosAsync();
        Task<ProductoDTO> GetProductoByIdAsync(int id);
        Task<ProductoDTO> CreateProductoAsync(CrearProductoDTO dto);
        Task<ProductoDTO> UpdateProductoAsync(int id, CrearProductoDTO dto);
        Task<bool> DeleteProductoAsync(int id);
        Task<(IEnumerable<ProductoDTO> Productos, int Total)> GetProductosAsync(int page, int pageSize, string? filtro);
    }
}
