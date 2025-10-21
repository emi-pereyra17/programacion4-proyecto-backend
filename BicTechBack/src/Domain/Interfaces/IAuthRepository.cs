using BicTechBack.src.Core.Entities;
using System;
using System.Threading.Tasks;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IAuthRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByIdAsync(int id);
        Task SaveRefreshTokenAsync(int id, string refreshToken, DateTime dateTime);
        Task<bool> UpdatePasswordAsync(int id, string newPassword);
        Task<Usuario?> GetUserByRefreshTokenAsync(string refreshToken);
    }
}
