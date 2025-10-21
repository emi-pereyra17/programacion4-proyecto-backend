using BicTechBack.src.Core.Entities;
using System;
using System.Threading.Tasks;

namespace BicTechBack.src.Core.Interfaces
{
    public interface IAuthRepository : IRepository<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task SaveRefreshTokenAsync(int id, string refreshToken, DateTime dateTime);
        Task<bool> UpdatePasswordAsync(int id, string newPassword);
        Task<Usuario?> GetUserByRefreshTokenAsync(string refreshToken);
    }
}