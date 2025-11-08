using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BicTechBack.src.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de usuarios del sistema.
    /// Solo accesible para administradores.
    /// </summary>
    [ApiController]
    [Route("usuarios")]
    [Authorize(Roles = "Admin")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de todos los usuarios.
        /// </summary>
        /// <returns>Lista de usuarios.</returns>
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            _logger.LogInformation("Obteniendo lista de todos los usuarios.");
            try
            {
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                _logger.LogInformation("Usuarios obtenidos correctamente. Total: {Total}", usuarios.Count());
                return Ok(new { message = "Lista de usuarios", usuarios });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar los usuarios.");
                return StatusCode(500, new { message = "Error al consultar los usuarios", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema.
        /// Solo accesible para administradores.
        /// </summary>
        /// <param name="dto">Datos del usuario a crear, incluyendo nombre, email, contraseña y rol.</param>
        /// <returns>El usuario creado o un mensaje de error si falla la operación.</returns>
        [HttpPost]
        public async Task<ActionResult> CreateUsuario([FromBody] CrearUsuarioDTO dto)
        {
            _logger.LogInformation("Intentando crear un nuevo usuario. Email: {Email}, Rol: {Rol}", dto.Email, dto.Rol);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos al intentar crear usuario. Email: {Email}", dto.Email);
                return BadRequest(new { message = "Datos inválidos. Verifica los campos requeridos." });
            }

            try
            {
                var usuarioCreado = await _usuarioService.CreateUsuarioAsync(dto, dto.Rol);
                _logger.LogInformation("Usuario creado correctamente. Email: {Email}, Rol: {Rol}", dto.Email, dto.Rol);
                return Ok(new
                {
                    message = "Usuario creado correctamente",
                    usuario = usuarioCreado
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error de operación al crear usuario. Email: {Email}, Error: {Error}", dto.Email, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario. Email: {Email}", dto.Email);
                return StatusCode(500, new { message = "Error interno al crear el usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de usuarios.
        /// </summary>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Cantidad de usuarios por página (por defecto 10).</param>
        /// <param name="filtro">Filtro opcional por nombre o email.</param>
        /// <returns>Lista paginada de usuarios y el total.</returns>
        [HttpGet("paginado")]
        public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? filtro = null)
        {
            _logger.LogInformation("Obteniendo usuarios paginados. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            try
            {
                var (usuarios, total) = await _usuarioService.GetUsuariosAsync(page, pageSize, filtro);
                _logger.LogInformation("Usuarios paginados obtenidos correctamente. Total: {Total}", total);
                return Ok(new
                {
                    message = "Lista paginada de usuarios",
                    total,
                    page,
                    pageSize,
                    usuarios
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar los usuarios paginados.");
                return StatusCode(500, new { message = "Error al consultar los usuarios", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un usuario por su identificador.
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <returns>Usuario encontrado o error si no existe.</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            _logger.LogInformation("Buscando usuario por Id: {Id}", id);
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                _logger.LogInformation("Usuario encontrado. Id: {Id}", id);
                return Ok(new { message = "Usuario encontrado", usuario });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Usuario no encontrado. Id: {Id}", id);
                return NotFound(new { message = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar el usuario. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al consultar el usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        /// <param name="id">Identificador del usuario a actualizar.</param>
        /// <param name="dto">Datos nuevos del usuario.</param>
        /// <returns>Usuario actualizado o error si no existe.</returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] CrearUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos requeridos faltantes al actualizar usuario. Id: {Id}", id);
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email, @"^\S+@\S+\.\S+$"))
            {
                _logger.LogWarning("Formato de email inválido al actualizar usuario. Id: {Id}, Email: {Email}", id, dto.Email);
                return BadRequest(new { message = "El email no tiene un formato válido" });
            }

            _logger.LogInformation("Intentando actualizar usuario. Id: {Id}, Email: {Email}", id, dto.Email);
            try
            {
                var usuarioActualizado = await _usuarioService.UpdateUsuarioAsync(dto, id);
                _logger.LogInformation("Usuario actualizado correctamente. Id: {Id}", id);
                return Ok(new { message = "Usuario actualizado", usuario = usuarioActualizado });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Usuario no encontrado al intentar actualizar. Id: {Id}", id);
                return NotFound(new { message = "Usuario no encontrado" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al actualizar usuario. Id: {Id}, Error: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un usuario por su identificador.
        /// </summary>
        /// <param name="id">Identificador del usuario a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            var roleClaim = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role" ||
                c.Type.EndsWith("/claims/role", StringComparison.OrdinalIgnoreCase)
            );

            foreach (var claim in User.Claims)
            {
                _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
            }

            if (userIdClaim == null || roleClaim == null)
            {
                return Forbid();
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("El claim de identificador no es un int válido: {Value}", userIdClaim.Value);
                return Forbid();
            }
            var userRole = roleClaim?.Value ?? "";

            _logger.LogInformation($"Rol detectado en claim: {userRole}");

            if (!string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase) && userId != id)
            {
                return Forbid();
            }

            _logger.LogInformation("Intentando eliminar usuario. Id: {Id}", id);
            try
            {
                var eliminado = await _usuarioService.DeleteUsuarioAsync(id);
                if (!eliminado)
                {
                    _logger.LogWarning("Usuario no encontrado al intentar eliminar. Id: {Id}", id);
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                _logger.LogInformation("Usuario eliminado correctamente. Id: {Id}", id);
                return Ok(new { message = "Usuario eliminado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar el usuario", error = ex.Message });
            }
        }
    }
}