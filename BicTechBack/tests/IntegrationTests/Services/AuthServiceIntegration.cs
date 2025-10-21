using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Mappings;
using BicTechBack.src.Core.Services;
using BicTechBack.src.Infrastructure.Data;
using BicTechBack.src.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BicTechBack.IntegrationTests.Services
{
    public class AuthServiceIntegration
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

        private IConfiguration GetConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "supersecretkeyforsigningjwt1234567890"},
                {"Jwt:Issuer", "testissuer"},
                {"Jwt:Audience", "testaudience"}
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        private AuthService GetService(AppDbContext context)
        {
            var authRepo = new AuthRepository(context);
            var usuarioRepo = new UsuarioRepository(context);
            var mapper = GetMapper();
            var config = GetConfiguration();
            var logger = new LoggerFactory().CreateLogger<AuthService>();
            return new AuthService(authRepo, mapper, usuarioRepo, config, logger);
        }

        /// <summary>
        /// Prueba que se pueda registrar un usuario correctamente.
        /// </summary>
        [Fact]
        public async Task RegisterUserAsync_UsuarioValido_RegistroCorrecto()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var dto = new RegisterUsuarioDTO
            {
                Nombre = "Nuevo",
                Email = "nuevo@mail.com",
                Password = "1234"
            };

            var id = await service.RegisterUserAsync(dto);

            Assert.True(id > 0);
            var usuarioEnDb = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "nuevo@mail.com");
            Assert.NotNull(usuarioEnDb);
            Assert.Equal("Nuevo", usuarioEnDb.Nombre);
        }

        /// <summary>
        /// Prueba que no se pueda registrar un usuario con un email ya existente.
        /// </summary>
        [Fact]
        public async Task RegisterUserAsync_EmailDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            context.Usuarios.Add(new Usuario
            {
                Nombre = "Existente",
                Email = "existente@mail.com",
                Password = "hashed",
                Rol = RolUsuario.User
            });
            await context.SaveChangesAsync();

            var dto = new RegisterUsuarioDTO
            {
                Nombre = "Nuevo",
                Email = "existente@mail.com",
                Password = "1234"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterUserAsync(dto));
        }

        /// <summary>
        /// Prueba que un usuario pueda iniciar sesión con credenciales válidas.
        /// </summary>
        [Fact]
        public async Task LoginUserAsync_UsuarioYPasswordValidos_RetornaLoginResultDTO()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            // Registrar usuario
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>();
            var usuario = new Usuario
            {
                Nombre = "Login",
                Email = "login@mail.com",
                Rol = RolUsuario.User
            };
            usuario.Password = passwordHasher.HashPassword(usuario, "1234");
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var dto = new LoginUsuarioDTO
            {
                Email = "login@mail.com",
                Password = "1234"
            };

            var result = await service.LoginUserAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(usuario.Email, result.Email);
            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        /// <summary>
        /// Prueba que no se pueda iniciar sesión con un email no registrado.
        /// </summary>
        [Fact]
        public async Task LoginUserAsync_EmailNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var dto = new LoginUsuarioDTO
            {
                Email = "noexiste@mail.com",
                Password = "1234"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.LoginUserAsync(dto));
        }

        /// <summary>
        /// Prueba que no se pueda iniciar sesión con una contraseña incorrecta.
        /// </summary>
        [Fact]
        public async Task LoginUserAsync_PasswordIncorrecto_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>();
            var usuario = new Usuario
            {
                Nombre = "Login",
                Email = "login@mail.com",
                Rol = RolUsuario.User
            };
            usuario.Password = passwordHasher.HashPassword(usuario, "correcta");
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var dto = new LoginUsuarioDTO
            {
                Email = "login@mail.com",
                Password = "incorrecta"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.LoginUserAsync(dto));
        }

        /// <summary>
        /// Prueba que se pueda actualizar la contraseña de un usuario existente.
        /// </summary>
        [Fact]
        public async Task UpdateUserPasswordAsync_UsuarioExistente_ActualizaYRetornaTrue()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var usuario = new Usuario
            {
                Nombre = "Cambio",
                Email = "cambio@mail.com",
                Password = "old",
                Rol = RolUsuario.User
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var result = await service.UpdateUserPasswordAsync(usuario.Id, "nuevaPassword");

            Assert.True(result);
        }

        /// <summary>
        /// Prueba que no se pueda actualizar la contraseña de un usuario no existente.
        /// </summary>
        [Fact]
        public async Task UpdateUserPasswordAsync_UsuarioNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateUserPasswordAsync(999, "nuevaPassword"));
        }

        /// <summary>
        /// Prueba que se pueda hacer logout correctamente (elimina el refresh token).
        /// </summary>
        [Fact]
        public async Task LogoutAsync_UsuarioValido_LogoutCorrecto()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var usuario = new Usuario
            {
                Nombre = "Logout",
                Email = "logout@mail.com",
                Password = "1234",
                Rol = RolUsuario.User,
                RefreshToken = "token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            await service.LogoutAsync(usuario.Id);

            var usuarioEnDb = await context.Usuarios.FindAsync(usuario.Id);
            Assert.Null(usuarioEnDb.RefreshToken);
        }

        /// <summary>
        /// Prueba que se pueda refrescar el token con un refresh token válido.
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_RefreshTokenValido_RetornaLoginResultDTO()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var usuario = new Usuario
            {
                Nombre = "Refresh",
                Email = "refresh@mail.com",
                Password = "1234",
                Rol = RolUsuario.User,
                RefreshToken = "refreshTokenValido",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var result = await service.RefreshTokenAsync("token", "refreshTokenValido");

            Assert.NotNull(result);
            Assert.Equal(usuario.Email, result.Email);
            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        /// <summary>
        /// Prueba que no se pueda refrescar el token con un refresh token inválido.
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_RefreshTokenInvalido_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RefreshTokenAsync("token", "refreshTokenInvalido"));
        }
    }
}