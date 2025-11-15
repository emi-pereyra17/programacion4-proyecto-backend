using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Mappings;
using BicTechBack.src.Core.Services;
using BicTechBack.src.Infrastructure.Data;
using BicTechBack.src.Infrastructure.Logging;
using BicTechBack.src.Infrastructure.Repositories;
using BicTechBack.src.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BicTechBack.IntegrationTests.Services
{
    public class UsuarioServiceIntegration
    {
        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UsuarioProfile>();
            });
            return config.CreateMapper();
        }

        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private UsuarioService GetService(AppDbContext context)
        {
            var repo = new UsuarioRepository(context);
            var mapper = GetMapper();
            var msLogger = new LoggerFactory().CreateLogger<UsuarioService>();
            var logger = new LoggerAdapter<UsuarioService>(msLogger);
            var passwordHasher = new PasswordHasherService(); 
            return new UsuarioService(repo, mapper, logger, passwordHasher);
        }

        /// <summary>
        /// Prueba que se pueda crear un usuario válido y que realmente se guarde en la base de datos.
        /// </summary>
        [Fact]
        public async Task CreateUsuarioAsync_UsuarioValido_PersisteEnDb()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Integracion",
                Email = "integ@mail.com",
                Password = "1234"
            };

            var result = await service.CreateUsuarioAsync(dto, "User");

            Assert.NotNull(result);
            Assert.Equal("Integracion", result.Nombre);
            Assert.Equal("integ@mail.com", result.Email);
            Assert.Equal("User", result.Rol);

            var usuarioEnDb = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "integ@mail.com");
            Assert.NotNull(usuarioEnDb);
            Assert.Equal("Integracion", usuarioEnDb.Nombre);
        }

        [Fact]
        public async Task CreateUsuarioAsync_EmailDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            context.Usuarios.Add(new Usuario
            {
                Nombre = "Existente",
                Email = "existente@mail.com",
                Password = "1234",
                Rol = RolUsuario.User
            });
            await context.SaveChangesAsync();

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Nuevo",
                Email = "existente@mail.com",
                Password = "abcd"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateUsuarioAsync(dto, "User"));
        }

        [Fact]
        public async Task CreateUsuarioAsync_RolVacio_LanzaArgumentException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Test",
                Email = "test@mail.com",
                Password = "1234"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateUsuarioAsync(dto, ""));
        }

        [Fact]
        public async Task CreateUsuarioAsync_RolInvalido_LanzaArgumentException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Test",
                Email = "test@mail.com",
                Password = "1234"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateUsuarioAsync(dto, "RolInvalido"));
        }

        [Fact]
        public async Task DeleteUsuarioAsync_UsuarioExistente_EliminaDeDb()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var usuario = new Usuario
            {
                Nombre = "Eliminar",
                Email = "eliminar@mail.com",
                Password = "1234",
                Rol = RolUsuario.User
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var result = await service.DeleteUsuarioAsync(usuario.Id);

            Assert.True(result);
            var usuarioEnDb = await context.Usuarios.FindAsync(usuario.Id);
            Assert.Null(usuarioEnDb);
        }

        [Fact]
        public async Task DeleteUsuarioAsync_UsuarioNoExistente_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteUsuarioAsync(999));
        }

        [Fact]
        public async Task GetAllUsuariosAsync_UsuariosExistentes_RetornaTodos()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            context.Usuarios.AddRange(
                new Usuario { Nombre = "A", Email = "a@mail.com", Password = "1", Rol = RolUsuario.User },
                new Usuario { Nombre = "B", Email = "b@mail.com", Password = "2", Rol = RolUsuario.Admin }
            );
            await context.SaveChangesAsync();

            var result = await service.GetAllUsuariosAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, u => u.Email == "a@mail.com");
            Assert.Contains(result, u => u.Email == "b@mail.com");
        }

        [Fact]
        public async Task GetAllUsuariosAsync_SinUsuarios_RetornaVacio()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllUsuariosAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsuarioByIdAsync_UsuarioExistente_RetornaUsuario()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var usuario = new Usuario
            {
                Nombre = "Buscar",
                Email = "buscar@mail.com",
                Password = "1234",
                Rol = RolUsuario.User
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var result = await service.GetUsuarioByIdAsync(usuario.Id);

            Assert.NotNull(result);
            Assert.Equal("Buscar", result.Nombre);
            Assert.Equal("buscar@mail.com", result.Email);
        }

        [Fact]
        public async Task GetUsuarioByIdAsync_UsuarioNoExistente_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetUsuarioByIdAsync(999));
        }

        [Fact]
        public async Task GetUsuariosAsync_Paginado_RetornaUsuariosYTotal()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            context.Usuarios.AddRange(
                new Usuario { Nombre = "A", Email = "a@mail.com", Password = "1", Rol = RolUsuario.User },
                new Usuario { Nombre = "B", Email = "b@mail.com", Password = "2", Rol = RolUsuario.Admin },
                new Usuario { Nombre = "C", Email = "c@mail.com", Password = "3", Rol = RolUsuario.User }
            );
            await context.SaveChangesAsync();

            var (usuarios, total) = await service.GetUsuariosAsync(page: 1, pageSize: 2, filtro: null);

            Assert.NotNull(usuarios);
            Assert.Equal(2, usuarios.Count());
            Assert.Equal(3, total);
        }

        [Fact]
        public async Task UpdateUsuarioAsync_UsuarioExistente_ActualizaDatos()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var usuario = new Usuario
            {
                Nombre = "Antiguo",
                Email = "antiguo@mail.com",
                Password = "old",
                Rol = RolUsuario.User
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Nuevo",
                Email = "nuevo@mail.com",
                Password = "new"
            };

            var result = await service.UpdateUsuarioAsync(dto, usuario.Id);

            Assert.NotNull(result);
            Assert.Equal("Nuevo", result.Nombre);
            Assert.Equal("nuevo@mail.com", result.Email);

            var usuarioEnDb = await context.Usuarios.FindAsync(usuario.Id);
            Assert.Equal("Nuevo", usuarioEnDb.Nombre);
            Assert.Equal("nuevo@mail.com", usuarioEnDb.Email);
        }

        [Fact]
        public async Task UpdateUsuarioAsync_UsuarioNoExistente_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Nuevo",
                Email = "nuevo@mail.com",
                Password = "new"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateUsuarioAsync(dto, 999));
        }

        [Fact]
        public async Task UpdateUsuarioAsync_EmailDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var usuario1 = new Usuario
            {
                Nombre = "Usuario1",
                Email = "usuario1@mail.com",
                Password = "1234",
                Rol = RolUsuario.User
            };
            var usuario2 = new Usuario
            {
                Nombre = "Usuario2",
                Email = "usuario2@mail.com",
                Password = "abcd",
                Rol = RolUsuario.Admin
            };
            context.Usuarios.AddRange(usuario1, usuario2);
            await context.SaveChangesAsync();

            var dto = new CrearUsuarioDTO
            {
                Nombre = "Nuevo",
                Email = "usuario2@mail.com",
                Password = "new"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateUsuarioAsync(dto, usuario1.Id));
        }
    }
}
