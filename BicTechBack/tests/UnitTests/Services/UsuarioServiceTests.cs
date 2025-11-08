using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BicTechBack.UnitTests.Services
{
    public class UsuarioServiceTests
    {
        [Fact]
        public async Task CreateUsuarioAsync_UsuarioValido_CreaUsuarioYRetornaDTO()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Test",
                Email = "test@mail.com",
                Password = "1234"
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Usuario>());
            mockMapper.Setup(m => m.Map<Usuario>(dto)).Returns(new Usuario { Nombre = dto.Nombre, Email = dto.Email, Password = dto.Password });
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<Usuario>())).ReturnsAsync(1);
            mockMapper.Setup(m => m.Map<UsuarioDTO>(It.IsAny<Usuario>())).Returns(new UsuarioDTO { Id = 1, Nombre = dto.Nombre, Email = dto.Email, Rol = "User" });

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            var result = await service.CreateUsuarioAsync(dto, "User");

            Assert.NotNull(result);
            Assert.Equal(dto.Nombre, result.Nombre);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal("User", result.Rol);
            mockRepo.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task CreateUsuarioAsync_EmailDuplicado_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Test",
                Email = "test@mail.com",
                Password = "1234"
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Usuario> { new Usuario { Id = 1, Email = dto.Email } });

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateUsuarioAsync(dto, "Cliente"));
        }

        [Fact]
        public async Task CreateUsuarioAsync_RolVacio_LanzaArgumentException()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Test",
                Email = "test@mail.com",
                Password = "1234"
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Usuario>());

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateUsuarioAsync(dto, ""));
        }

        [Fact]
        public async Task CreateUsuarioAsync_RolInvalido_LanzaArgumentException()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Test",
                Email = "test@mail.com",
                Password = "1234"
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Usuario>());
            mockMapper.Setup(m => m.Map<Usuario>(dto)).Returns(new Usuario { Nombre = dto.Nombre, Email = dto.Email, Password = dto.Password });

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateUsuarioAsync(dto, "RolInvalido"));
        }

        [Fact]
        public async Task DeleteUsuarioAsync_UsuarioExistente_EliminaYRetornaTrue()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            var usuario = new Usuario { Id = 1, Nombre = "Test", Email = "test@mail.com" };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            var result = await service.DeleteUsuarioAsync(1);

            Assert.True(result);
            mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteUsuarioAsync_UsuarioNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Usuario?)null);

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteUsuarioAsync(1));
        }

        [Fact]
        public async Task GetAllUsuariosAsync_UsuariosExistentes_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            var usuarios = new List<Usuario>
            {
                new Usuario { Id = 1, Nombre = "Test1", Email = "test1@mail.com" },
                new Usuario { Id = 2, Nombre = "Test2", Email = "test2@mail.com" }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(usuarios);
            mockMapper.Setup(m => m.Map<IEnumerable<UsuarioDTO>>(It.IsAny<IEnumerable<Usuario>>()))
                .Returns(new List<UsuarioDTO>
                {
                    new UsuarioDTO { Id = 1, Nombre = "Test1", Email = "test1@mail.com" },
                    new UsuarioDTO { Id = 2, Nombre = "Test2", Email = "test2@mail.com" }
                });

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            var result = await service.GetAllUsuariosAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllUsuariosAsync_NoUsuarios_RetornaListaVacia()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Usuario>());
            mockMapper.Setup(m => m.Map<IEnumerable<UsuarioDTO>>(It.IsAny<IEnumerable<Usuario>>()))
                .Returns(new List<UsuarioDTO>());

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            var result = await service.GetAllUsuariosAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsuarioByIdAsync_UsuarioExistente_RetornaDTO()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            var usuario = new Usuario { Id = 1, Nombre = "Test", Email = "test@mail.com" };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
            mockMapper.Setup(m => m.Map<UsuarioDTO>(It.IsAny<Usuario>())).Returns(new UsuarioDTO { Id = 1, Nombre = "Test", Email = "test@mail.com" });

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            var result = await service.GetUsuarioByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetUsuarioByIdAsync_UsuarioNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Usuario?)null);

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetUsuarioByIdAsync(1));
        }

        [Fact]
        public async Task UpdateUsuarioAsync_UsuarioActualizado_RetornaDTO()
        {
            var mockRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<UsuarioService>>();
            var mockHasher = new Mock<IPasswordHasher<Usuario>>();

            var usuarioExistente = new Usuario
            {
                Id = 1,
                Nombre = "Antiguo",
                Email = "antiguo@mail.com",
                Password = "old"
            };
            var dto = new CrearUsuarioDTO
            {
                Nombre = "Nuevo",
                Email = "nuevo@mail.com",
                Password = "new"
            };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuarioExistente);
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Usuario> { usuarioExistente });
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Usuario>())).ReturnsAsync(usuarioExistente);
            mockMapper.Setup(m => m.Map<UsuarioDTO>(It.IsAny<Usuario>())).Returns(new UsuarioDTO { Id = 1, Nombre = "Nuevo", Email = "nuevo@mail.com" });

            var service = new UsuarioService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockHasher.Object);

            var result = await service.UpdateUsuarioAsync(dto, 1);

            Assert.NotNull(result);
            Assert.Equal("Nuevo", result.Nombre);
            Assert.Equal("nuevo@mail.com", result.Email);
        }
    }
}
