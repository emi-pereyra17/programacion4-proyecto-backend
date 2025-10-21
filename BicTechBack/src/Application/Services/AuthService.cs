using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace BicTechBack.src.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger; 
        public AuthService(
            IAuthRepository repository,
            IMapper mapper,
            IUsuarioRepository usuarioRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger) 
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResultDTO> LoginUserAsync(LoginUsuarioDTO dto)
        {
            _logger.LogInformation("Intentando login para el usuario: {Email}", dto.Email);

            var usuario = await _repository.GetByEmailAsync(dto.Email);
            if (usuario == null)
            {
                _logger.LogWarning("Intento de login con email no registrado: {Email}", dto.Email);
                throw new InvalidOperationException("Credenciales inválidas.");
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(usuario, usuario.Password, dto.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Intento de login fallido por contraseña incorrecta. Email: {Email}", dto.Email);
                throw new InvalidOperationException("Credenciales inválidas.");
            }

            var token = GenerateJwtToken(usuario);
            var refreshToken = GenerateRefreshToken();

            await _repository.SaveRefreshTokenAsync(usuario.Id, refreshToken, DateTime.UtcNow.AddDays(7));

            _logger.LogInformation("Login exitoso para el usuario: {Email}", dto.Email);

            return new LoginResultDTO
            {
                Token = token,
                RefreshToken = refreshToken,
                UsuarioId = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol.ToString()
            };
        }

        public async Task LogoutAsync(int userId)
        {
            _logger.LogInformation("Logout solicitado para el usuario Id: {UserId}", userId);
            await _repository.SaveRefreshTokenAsync(userId, null, DateTime.MinValue);
            _logger.LogInformation("Logout realizado para el usuario Id: {UserId}", userId);
        }

        public async Task<int> RegisterUserAsync(RegisterUsuarioDTO dto)
        {
            _logger.LogInformation("Intentando registrar usuario: {Email}", dto.Email);

            var usuarioExistente = await _repository.GetByEmailAsync(dto.Email);
            if (usuarioExistente != null)
            {
                _logger.LogWarning("Intento de registro con email ya existente: {Email}", dto.Email);
                throw new InvalidOperationException("El usuario ya existe con ese email.");
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Rol = RolUsuario.User
            };

            usuario.Password = _passwordHasher.HashPassword(usuario, dto.Password);

            var usuarioCreado = await _usuarioRepository.CreateAsync(usuario);

            _logger.LogInformation("Usuario registrado correctamente. Id: {Id}, Email: {Email}", usuarioCreado, usuario.Email);

            return usuarioCreado;
        }

        public async Task<bool> UpdateUserPasswordAsync(int id, string newPassword)
        {
            _logger.LogInformation("Actualizando contraseña para el usuario Id: {UserId}", id);

            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null)
            {
                _logger.LogWarning("Intento de actualizar contraseña para usuario no encontrado. Id: {UserId}", id);
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            var hashedPassword = _passwordHasher.HashPassword(usuario, newPassword);
            var resultado = await _repository.UpdatePasswordAsync(id, hashedPassword);

            if (resultado)
                _logger.LogInformation("Contraseña actualizada correctamente para el usuario Id: {UserId}", id);
            else
                _logger.LogWarning("No se pudo actualizar la contraseña para el usuario Id: {UserId}", id);

            return resultado;
        }

        public async Task<LoginResultDTO> RefreshTokenAsync(string token, string refreshToken)
        {
            _logger.LogInformation("Intentando refresh de token.");

            var usuario = await _repository.GetUserByRefreshTokenAsync(refreshToken);
            if (usuario == null)
            {
                _logger.LogWarning("Refresh token inválido o expirado.");
                throw new InvalidOperationException("Refresh token inválido.");
            }

            var newToken = GenerateJwtToken(usuario);
            var newRefreshToken = GenerateRefreshToken();
            await _repository.SaveRefreshTokenAsync(usuario.Id, newRefreshToken, DateTime.UtcNow.AddDays(7));

            _logger.LogInformation("Refresh de token exitoso para el usuario Id: {UserId}", usuario.Id);

            return new LoginResultDTO
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                UsuarioId = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email
            };
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim("nombre", usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
