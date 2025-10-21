using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<int> CreateAsync(Usuario usuario); 
    }
}
