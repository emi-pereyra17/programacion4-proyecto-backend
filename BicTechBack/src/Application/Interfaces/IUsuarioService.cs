using BicTechBack.src.Core.DTOs;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDTO>> GetAllUsuariosAsync();
        Task<UsuarioDTO> GetUsuarioByIdAsync(int id);
        Task<UsuarioDTO> CreateUsuarioAsync(CrearUsuarioDTO dto, string rol);
        Task<UsuarioDTO> UpdateUsuarioAsync(CrearUsuarioDTO dto, int id);
        Task<bool> DeleteUsuarioAsync(int id);
        Task<(IEnumerable<UsuarioDTO> Usuarios, int Total)> GetUsuariosAsync(int page, int pageSize, string? filtro);
    }
}
