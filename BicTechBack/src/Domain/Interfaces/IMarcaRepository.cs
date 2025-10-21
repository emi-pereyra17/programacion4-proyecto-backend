using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IMarcaRepository : IRepository<Marca>
    {
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null);
    }
}