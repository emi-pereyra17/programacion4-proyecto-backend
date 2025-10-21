using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Interfaces
{
    public interface ICategoriaMarcaRepository : IRepository<CategoriaMarca>
    {
        Task<IEnumerable<CategoriaMarca>> GetByCategoriaIdAsync(int categoriaId);
        Task<IEnumerable<CategoriaMarca>> GetByMarcaIdAsync(int marcaId);
    }
}