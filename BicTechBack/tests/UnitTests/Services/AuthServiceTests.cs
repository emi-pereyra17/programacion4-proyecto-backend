using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BicTechBack.UnitTests.Services
{
    public class AuthServiceTests
    {
        private Mock<IAuthRepository> mockAuthRepo = new();
        private Mock<IUsuarioRepository> mockUsuarioRepo = new();
        private Mock<IMapper> mockMapper = new();
        private Mock<IConfiguration> mockConfig = new();
        private Mock<ILogger<AuthService>> mockLogger = new();

        private AuthService CreateService()
        {
            // Configuración mínima para JWT
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("supersecretkeyforsigningjwt1234567890");
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("testissuer");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("testaudience");
            return new AuthService(
                mockAuthRepo.Object,
                mockMapper.Object,
                mockUsuarioRepo.Object,
                mockConfig.Object,
                mockLogger.Object
            );
        }

        [Fact]
        public async Task RegisterUserAsync_UsuarioValido_RegistroCorrecto()
        {
            var dto = new RegisterUsuarioDTO { Nombre = "Test", Email = "test@mail.com", Password = "1234" };
            mockAuthRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((Usuario?)null);
            mockUsuarioRepo.Setup(r => r.CreateAsync(It.IsAny<Usuario>())).ReturnsAsync(1);

            var service = CreateService();
            var result = await service.RegisterUserAsync(dto);

            Assert.Equal(1, result);
            mockUsuarioRepo.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_EmailDuplicado_LanzaInvalidOperationException()
        {
            var dto = new RegisterUsuarioDTO { Nombre = "Test", Email = "test@mail.com", Password = "1234" };
            mockAuthRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new Usuario { Id = 1, Email = dto.Email });

            var service = CreateService();
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterUserAsync(dto));
        }

        [Fact]
        public async Task LoginUserAsync_UsuarioYPasswordValidos_RetornaLoginResultDTO()
        {
            var dto = new LoginUsuarioDTO { Email = "test@mail.com", Password = "1234" };
            var usuario = new Usuario { Id = 1, Nombre = "Test", Email = dto.Email, Password = "" };
            var passwordHasher = new PasswordHasher<Usuario>();
            usuario.Password = passwordHasher.HashPassword(usuario, dto.Password);

            mockAuthRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(usuario);
            mockAuthRepo.Setup(r => r.SaveRefreshTokenAsync(usuario.Id, It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("supersecretkeyforsigningjwt1234567890");
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("testissuer");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("testaudience");

            var service = CreateService();
            var result = await service.LoginUserAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(usuario.Id, result.UsuarioId);
            Assert.Equal(usuario.Email, result.Email);
            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        [Fact]
        public async Task LoginUserAsync_EmailNoExiste_LanzaInvalidOperationException()
        {
            var dto = new LoginUsuarioDTO { Email = "test@mail.com", Password = "1234" };
            mockAuthRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((Usuario?)null);

            var service = CreateService();
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.LoginUserAsync(dto));
        }

        [Fact]
        public async Task LoginUserAsync_PasswordIncorrecto_LanzaInvalidOperationException()
        {
            var dto = new LoginUsuarioDTO { Email = "test@mail.com", Password = "incorrecta" };
            var usuario = new Usuario { Id = 1, Nombre = "Test", Email = dto.Email, Password = "" };
            var passwordHasher = new PasswordHasher<Usuario>();
            usuario.Password = passwordHasher.HashPassword(usuario, "correcta");

            mockAuthRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(usuario);

            var service = CreateService();
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.LoginUserAsync(dto));
        }

        [Fact]
        public async Task LogoutAsync_UsuarioValido_LogoutCorrecto()
        {
            mockAuthRepo.Setup(r => r.SaveRefreshTokenAsync(1, null, DateTime.MinValue)).Returns(Task.CompletedTask);

            var service = CreateService();
            await service.LogoutAsync(1);

            mockAuthRepo.Verify(r => r.SaveRefreshTokenAsync(1, null, DateTime.MinValue), Times.Once);
        }

        [Fact]
        public async Task UpdateUserPasswordAsync_UsuarioExistente_ActualizaYRetornaTrue()
        {
            var usuario = new Usuario { Id = 1, Nombre = "Test", Email = "test@mail.com", Password = "" };
            mockAuthRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
            mockAuthRepo.Setup(r => r.UpdatePasswordAsync(1, It.IsAny<string>())).ReturnsAsync(true);

            var service = CreateService();
            var result = await service.UpdateUserPasswordAsync(1, "nuevaPassword");

            Assert.True(result);
            mockAuthRepo.Verify(r => r.UpdatePasswordAsync(1, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserPasswordAsync_UsuarioNoExiste_LanzaInvalidOperationException()
        {
            mockAuthRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Usuario?)null);

            var service = CreateService();
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateUserPasswordAsync(1, "nuevaPassword"));
        }

        [Fact]
        public async Task UpdateUserPasswordAsync_UpdateFalla_RetornaFalse()
        {
            var usuario = new Usuario { Id = 1, Nombre = "Test", Email = "test@mail.com", Password = "" };
            mockAuthRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
            mockAuthRepo.Setup(r => r.UpdatePasswordAsync(1, It.IsAny<string>())).ReturnsAsync(false);

            var service = CreateService();
            var result = await service.UpdateUserPasswordAsync(1, "nuevaPassword");

            Assert.False(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_RefreshTokenValido_RetornaLoginResultDTO()
        {
            var usuario = new Usuario { Id = 1, Nombre = "Test", Email = "test@mail.com", Password = "" };
            mockAuthRepo.Setup(r => r.GetUserByRefreshTokenAsync("refreshTokenValido")).ReturnsAsync(usuario);
            mockAuthRepo.Setup(r => r.SaveRefreshTokenAsync(usuario.Id, It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("supersecretkeyforsigningjwt1234567890");
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("testissuer");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("testaudience");

            var service = CreateService();
            var result = await service.RefreshTokenAsync("token", "refreshTokenValido");

            Assert.NotNull(result);
            Assert.Equal(usuario.Id, result.UsuarioId);
            Assert.Equal(usuario.Email, result.Email);
            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        [Fact]
        public async Task RefreshTokenAsync_RefreshTokenInvalido_LanzaInvalidOperationException()
        {
            mockAuthRepo.Setup(r => r.GetUserByRefreshTokenAsync("refreshTokenInvalido")).ReturnsAsync((Usuario?)null);

            var service = CreateService();
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RefreshTokenAsync("token", "refreshTokenInvalido"));
        }
    }
}