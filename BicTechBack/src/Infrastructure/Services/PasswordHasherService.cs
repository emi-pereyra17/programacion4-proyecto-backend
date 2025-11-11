using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BicTechBack.src.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Infrastructure.Security
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public PasswordHasherService()
        {
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        public string HashPassword(string password)
        {
            var dummyUser = new Usuario();
            return _passwordHasher.HashPassword(dummyUser, password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var dummyUser = new Usuario();
            var result = _passwordHasher.VerifyHashedPassword(dummyUser, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}