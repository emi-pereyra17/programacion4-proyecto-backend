using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BicTechBack.src.API.Controllers
{
    /// <summary>
    /// Controlador para la autenticación y gestión de usuarios.
    /// Proporciona endpoints para registro, login, logout, actualización de contraseña y refresh de tokens.
    /// </summary>
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="dto">Datos del usuario a registrar.</param>
        /// <returns>Id del usuario registrado.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register([FromBody] RegisterUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos requeridos faltantes al registrar usuario.");
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando registrar usuario: {Email}", dto.Email);
            try
            {
                var id = await _authService.RegisterUserAsync(dto);
                _logger.LogInformation("Usuario registrado correctamente. Id: {Id}, Email: {Email}", id, dto.Email);
                return StatusCode(201, new { message = "Usuario registrado", id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Intento de registro con email ya existente: {Email}", dto.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el usuario. Email: {Email}", dto.Email);
                return StatusCode(500, new { message = "Error al registrar el usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Inicia sesión para un usuario registrado.
        /// </summary>
        /// <param name="dto">Credenciales de acceso del usuario.</param>
        /// <returns>Token JWT y datos del usuario autenticado.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LoginUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos requeridos faltantes al iniciar sesión.");
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando login para el usuario: {Email}", dto.Email);
            try
            {
                var result = await _authService.LoginUserAsync(dto);
                _logger.LogInformation("Login exitoso para el usuario: {Email}", dto.Email);
                return Ok(new { token = result.Token, user = new { id = result.UsuarioId, nombre = result.Nombre, email = result.Email,
                    rol = result.Rol
                } });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Intento de login fallido para el usuario: {Email}", dto.Email);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión. Email: {Email}", dto.Email);
                return StatusCode(500, new { message = "Error al iniciar sesión", error = ex.Message });
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario autenticado.
        /// </summary>
        /// <returns>Mensaje de confirmación de cierre de sesión.</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            _logger.LogInformation("Logout solicitado para el usuario Id: {UserId}", userId);
            await _authService.LogoutAsync(userId);
            _logger.LogInformation("Logout realizado para el usuario Id: {UserId}", userId);
            return Ok(new { message = "Sesión cerrada correctamente" });
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario.
        /// Solo el usuario propietario o un administrador pueden realizar esta acción.
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <param name="password">Nueva contraseña.</param>
        /// <returns>Mensaje de confirmación o error.</returns>
        [HttpPut("password/{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult> UpdatePassword(int id, [FromBody] string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Contraseña inválida al intentar actualizar. Id: {Id}", id);
                return BadRequest(new { message = "Contraseña inválida" });
            }

            var isAdmin = User.IsInRole("Admin");
            var userIdClaim = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            if (!isAdmin && id != userIdClaim)
            {
                _logger.LogWarning("Usuario no autorizado para actualizar contraseña. Id: {Id}", id);
                return Forbid();
            }

            _logger.LogInformation("Intentando actualizar contraseña para el usuario Id: {Id}", id);
            try
            {
                var actualizado = await _authService.UpdateUserPasswordAsync(id, password);
                if (!actualizado)
                {
                    _logger.LogWarning("Usuario no encontrado al intentar actualizar contraseña. Id: {Id}", id);
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                _logger.LogInformation("Contraseña actualizada correctamente para el usuario Id: {Id}", id);
                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la contraseña. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar la contraseña", error = ex.Message });
            }
        }

        /// <summary>
        /// Renueva el token de autenticación usando un refresh token válido.
        /// </summary>
        /// <param name="dto">Datos del token y refresh token.</param>
        /// <returns>Nuevo token y datos del usuario autenticado.</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult> Refresh([FromBody] RefreshRequestDTO dto)
        {
            _logger.LogInformation("Intentando refresh de token.");
            try
            {
                var result = await _authService.RefreshTokenAsync(dto.Token, dto.RefreshToken);
                _logger.LogInformation("Refresh de token exitoso para el usuario Id: {UserId}", result.UsuarioId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Refresh token inválido o expirado.");
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}

