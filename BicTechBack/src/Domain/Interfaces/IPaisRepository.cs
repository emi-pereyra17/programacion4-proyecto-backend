using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IPaisRepository
    {
        Task<IEnumerable<Pais>> GetAllAsync();
        Task<Pais?> GetByIdAsync(int id);
        Task<Pais> AddAsync(Pais pais);
        Task<Pais> UpdateAsync(Pais pais);
        Task<bool> DeleteAsync(int id);
    }
}
