using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BicTechBack.src.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de carritos de compras de los usuarios.
    /// </summary>
    [ApiController]
    [Route("carritos")]
    public class CarritoController : ControllerBase
    {
        private readonly ICarritoService _carritoService;
        private readonly ILogger<CarritoController> _logger;

        public CarritoController(ICarritoService carritoService, ILogger<CarritoController> logger)
        {
            _carritoService = carritoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de todos los carritos.
        /// Solo accesible para administradores.
        /// </summary>
        /// <returns>Lista de carritos.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Obteniendo todos los carritos.");
            var carritos = await _carritoService.GetAllCarritosAsync();
            _logger.LogInformation("Carritos obtenidos correctamente. Total: {Total}", carritos.Count());
            return Ok(carritos);
        }

        /// <summary>
        /// Obtiene una lista paginada de carritos.
        /// </summary>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Cantidad de carritos por página (por defecto 10).</param>
        /// <param name="filtro">Filtro opcional por usuario.</param>
        /// <returns>Lista paginada de carritos y el total.</returns>
        [HttpGet("paginado")]
        public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? filtro = null)
        {
            _logger.LogInformation("Obteniendo carritos paginados. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            try
            {
                var (carritos, total) = await _carritoService.GetCarritosAsync(page, pageSize, filtro);
                _logger.LogInformation("Carritos paginados obtenidos correctamente. Total: {Total}", total);
                return Ok(new
                {
                    message = "Lista paginada de carritos",
                    total,
                    page,
                    pageSize,
                    carritos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar los carritos paginados.");
                return StatusCode(500, new { message = "Error al consultar los carritos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el carrito de un usuario específico.
        /// Solo el usuario propietario o un administrador puede acceder.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <returns>Carrito del usuario o una lista vacía si no existe.</returns>
        [HttpGet("{usuarioId:int}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            var isAdmin = User.IsInRole("Admin");
            var userIdClaim = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            if (!isAdmin && usuarioId != userIdClaim)
            {
                _logger.LogWarning("Usuario no autorizado para acceder al carrito. UsuarioId: {UsuarioId}", usuarioId);
                return Forbid();
            }
            _logger.LogInformation("Obteniendo carrito para el usuario {UsuarioId}", usuarioId);
            var carrito = await _carritoService.GetCarritoByUsuarioIdAsync(usuarioId);
            if (carrito == null || carrito.Id == 0)
            {
                _logger.LogInformation("No se encontró carrito para el usuario {UsuarioId}", usuarioId);
                return Ok(new { usuarioId, productos = new List<object>() });
            }
            _logger.LogInformation("Carrito encontrado para el usuario {UsuarioId}", usuarioId);
            return Ok(carrito);
        }

        /// <summary>
        /// Actualiza la cantidad de un producto en el carrito de un usuario.
        /// Solo el usuario propietario o un administrador puede modificarlo.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <param name="productoId">Identificador del producto.</param>
        /// <param name="cantidad">Nueva cantidad del producto.</param>
        /// <returns>Carrito actualizado.</returns>
        [HttpPut("{usuarioId:int}/productos/{productoId:int}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> UpdateAmountProductoInCarrito(int usuarioId, int productoId, [FromQuery] int cantidad)
        {
            var isAdmin = User.IsInRole("Admin");
            var userIdClaim = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            if (!isAdmin && usuarioId != userIdClaim)
            {
                _logger.LogWarning("Usuario no autorizado para modificar el carrito. UsuarioId: {UsuarioId}", usuarioId);
                return Forbid();
            }
            if (cantidad <= 0)
            {
                _logger.LogWarning("Cantidad inválida al actualizar producto en carrito. UsuarioId: {UsuarioId}, ProductoId: {ProductoId}, Cantidad: {Cantidad}", usuarioId, productoId, cantidad);
                return BadRequest(new { message = "La cantidad debe ser mayor que cero." });
            }

            _logger.LogInformation("Actualizando cantidad del producto {ProductoId} en el carrito del usuario {UsuarioId} a {Cantidad}", productoId, usuarioId, cantidad);
            try
            {
                var carritoActualizado = await _carritoService.UpdateAmountProductoAsync(usuarioId, productoId, cantidad);
                _logger.LogInformation("Cantidad actualizada correctamente en el carrito del usuario {UsuarioId}", usuarioId);
                return Ok(new { message = "Cantidad actualizada", carrito = carritoActualizado });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al actualizar cantidad en carrito. UsuarioId: {UsuarioId}, ProductoId: {ProductoId}, Error: {Error}", usuarioId, productoId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Agrega un producto al carrito de un usuario.
        /// Solo el usuario propietario o un administrador puede agregar productos.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <param name="productoId">Identificador del producto.</param>
        /// <param name="cantidad">Cantidad del producto a agregar.</param>
        /// <returns>Carrito actualizado.</returns>
        [HttpPost("{usuarioId:int}/productos/{productoId:int}/add")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> AddProductoToCarrito(int usuarioId, int productoId, [FromQuery] int cantidad)
        {
            var isAdmin = User.IsInRole("Admin");
            var userIdClaim = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            if (!isAdmin && usuarioId != userIdClaim)
            {
                _logger.LogWarning("Usuario no autorizado para agregar producto al carrito. UsuarioId: {UsuarioId}", usuarioId);
                return Forbid();
            }
            if (cantidad <= 0)
            {
                _logger.LogWarning("Cantidad inválida al agregar producto al carrito. UsuarioId: {UsuarioId}, ProductoId: {ProductoId}, Cantidad: {Cantidad}", usuarioId, productoId, cantidad);
                return BadRequest(new { message = "La cantidad debe ser mayor que cero." });
            }

            _logger.LogInformation("Agregando producto {ProductoId} al carrito del usuario {UsuarioId} con cantidad {Cantidad}", productoId, usuarioId, cantidad);
            try
            {
                var carritoCreado = await _carritoService.AddProductoToCarritoAsync(usuarioId, productoId, cantidad);
                _logger.LogInformation("Producto agregado correctamente al carrito del usuario {UsuarioId}", usuarioId);
                return Ok(new { message = "Producto agregado al carrito", carrito = carritoCreado });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al agregar producto al carrito. UsuarioId: {UsuarioId}, ProductoId: {ProductoId}, Error: {Error}", usuarioId, productoId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // ...
        /// <summary>
        /// Elimina un producto del carrito de un usuario.
        /// Solo el usuario propietario o un administrador puede eliminar productos.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <param name="productoId">Identificador del producto a eliminar.</param>
        /// <returns>Carrito actualizado.</returns>
        [HttpDelete("{usuarioId:int}/productos/{productoId:int}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> DeleteProductoFromCarrito(int usuarioId, int productoId)
        {
            var isAdmin = User.IsInRole("Admin");
            var userIdClaim = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            if (!isAdmin && usuarioId != userIdClaim)
            {
                _logger.LogWarning("Usuario no autorizado para eliminar producto del carrito. UsuarioId: {UsuarioId}", usuarioId);
                return Forbid();
            }

            _logger.LogInformation("Eliminando producto {ProductoId} del carrito del usuario {UsuarioId}", productoId, usuarioId);
            try
            {
                var eliminado = await _carritoService.DeleteProductoFromCarritoAsync(usuarioId, productoId);
                _logger.LogInformation("Producto eliminado correctamente del carrito del usuario {UsuarioId}", usuarioId);
                return Ok(new { message = "Producto quitado del carrito", carrito = eliminado });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al eliminar producto del carrito. UsuarioId: {UsuarioId}, ProductoId: {ProductoId}, Error: {Error}", usuarioId, productoId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Vacía el carrito de un usuario.
        /// Solo el usuario propietario o un administrador puede vaciar el carrito.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <returns>Carrito vacío.</returns>
        [HttpDelete("{usuarioId:int}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> ClearCarrito(int usuarioId)
        {
            var isAdmin = User.IsInRole("Admin");
            var userIdClaim = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            if (!isAdmin && usuarioId != userIdClaim)
            {
                _logger.LogWarning("Usuario no autorizado para vaciar el carrito. UsuarioId: {UsuarioId}", usuarioId);
                return Forbid();
            }

            _logger.LogInformation("Limpiando carrito del usuario {UsuarioId}", usuarioId);
            try
            {
                var carritoLimpio = await _carritoService.ClearCarritoAsync(usuarioId);
                _logger.LogInformation("Carrito del usuario {UsuarioId} vaciado correctamente", usuarioId);
                return Ok(new { message = "Carrito vaciado", carrito = carritoLimpio });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al limpiar carrito. UsuarioId: {UsuarioId}, Error: {Error}", usuarioId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
