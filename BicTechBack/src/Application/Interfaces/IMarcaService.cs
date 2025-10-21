using BicTechBack.src.Core.DTOs;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IMarcaService
    {
        Task<IEnumerable<MarcaDTO>> GetAllMarcasAsync();
        Task<MarcaDTO> GetMarcaByIdAsync(int id);
        Task<MarcaDTO> CreateMarcaAsync(CrearMarcaDTO dto);
        Task<MarcaDTO> UpdateMarcaAsync(int id, CrearMarcaDTO dto);
        Task<bool> DeleteMarcaAsync(int id);
        Task<(IEnumerable<MarcaDTO> Marcas, int Total)> GetMarcasAsync(int page, int pageSize, string? filtro);
    }
}
