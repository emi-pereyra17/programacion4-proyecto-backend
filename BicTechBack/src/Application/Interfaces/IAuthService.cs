using BicTechBack.src.Core.DTOs;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IAuthService
    {
        Task<int> RegisterUserAsync(RegisterUsuarioDTO dto);
        Task<LoginResultDTO> LoginUserAsync(LoginUsuarioDTO dto);
        Task<bool> UpdateUserPasswordAsync(int id, string newPassword);
        Task<LoginResultDTO> RefreshTokenAsync(string token, string refreshToken);
        Task LogoutAsync(int userId);
    }
}
