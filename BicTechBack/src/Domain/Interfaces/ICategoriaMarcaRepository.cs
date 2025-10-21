using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Interfaces
{
    public interface ICategoriaMarcaRepository
    {
        Task<IEnumerable<CategoriaMarca>> GetAllAsync();
        Task<IEnumerable<CategoriaMarca>> GetByCategoriaIdAsync(int categoriaId);
        Task<IEnumerable<CategoriaMarca>> GetByMarcaIdAsync(int marcaId);
        Task<CategoriaMarca> AddAsync(CategoriaMarca categoriaMarca);
        Task<bool> DeleteAsync(int id);
    }
}
