using BicTechBack.src.Core.DTOs;

namespace BicTechBack.src.Core.Interfaces
{
    public interface ICategoriaMarcaService
    {
        Task<IEnumerable<CategoriaMarcaDTO>> GetAllCMAsync();
        Task<IEnumerable<CategoriaMarcaDTO>> GetCMByCategoriaIdAsync(int categoriaId);
        Task<IEnumerable<CategoriaMarcaDTO>> GetCMByMarcaIdAsync(int marcaId);
        Task<CategoriaMarcaDTO> CreateCMAsync(CrearCategoriaMarcaDTO dto);
        Task<bool> DeleteCMAsync(int id);
        Task<IEnumerable<MarcaDTO>> GetMarcasPorCategoriaAsync(int categoriaId);
        Task<IEnumerable<CategoriaDTO>> GetCategoriasPorMarcaAsync(int marcaId);
        Task<(IEnumerable<CategoriaMarcaDTO> CategoriasMarcas, int Total)> GetCMAsync(int page, int pageSize, string? filtro);
    }
}
