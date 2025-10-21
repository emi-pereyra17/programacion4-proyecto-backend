using BicTechBack.src.Core.DTOs;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IPaisService
    {
        Task<IEnumerable<PaisDTO>> GetAllPaisesAsync();
        Task<PaisDTO> GetPaisByIdAsync(int id);
        Task<PaisDTO> CreatePaisAsync(CrearPaisDTO dto);
        Task<PaisDTO> UpdatePaisAsync(int id, CrearPaisDTO dto);
        Task<bool> DeletePaisAsync(int id);
        Task<(IEnumerable<PaisDTO> Paises, int Total)> GetPaisesAsync(int page, int pageSize, string? filtro);
    }
}
