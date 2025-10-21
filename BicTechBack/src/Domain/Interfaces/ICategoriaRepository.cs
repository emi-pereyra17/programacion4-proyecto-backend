using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Interfaces
{
    public interface ICategoriaRepository : IRepository<Categoria>
    {
        Task<bool> ExistsAsync(int id);
    }
}