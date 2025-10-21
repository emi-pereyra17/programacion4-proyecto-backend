using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BicTechBack.src.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de productos.
    /// </summary>
    [ApiController]
    [Route("productos")]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly ILogger<ProductoController> _logger;

        public ProductoController(IProductoService productoService, ILogger<ProductoController> logger)
        {
            _productoService = productoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de todos los productos.
        /// </summary>
        /// <returns>Lista de productos.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll()
        {
            _logger.LogInformation("Obteniendo lista de todos los productos.");
            try
            {
                var productos = await _productoService.GetAllProductosAsync();
                _logger.LogInformation("Productos obtenidos correctamente. Total: {Total}", productos.Count());
                return Ok(new { message = "Lista de productos", productos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar los productos.");
                return StatusCode(500, new { message = "Error al consultar los productos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de productos.
        /// </summary>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Cantidad de productos por página (por defecto 10).</param>
        /// <param name="filtro">Filtro opcional por nombre, categoría, etc.</param>
        /// <returns>Lista paginada de productos y el total.</returns>
        [HttpGet("paginado")]
        public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? filtro = null)
        {
            _logger.LogInformation("Obteniendo productos paginados. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            try
            {
                var (productos, total) = await _productoService.GetProductosAsync(page, pageSize, filtro);
                _logger.LogInformation("Productos paginados obtenidos correctamente. Total: {Total}", total);
                return Ok(new
                {
                    message = "Lista paginada de productos",
                    total,
                    page,
                    pageSize,
                    productos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar los productos paginados.");
                return StatusCode(500, new { message = "Error al consultar los productos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un producto por su identificador.
        /// </summary>
        /// <param name="id">Identificador del producto.</param>
        /// <returns>Producto encontrado o error si no existe.</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById(int id)
        {
            _logger.LogInformation("Buscando producto por Id: {Id}", id);
            try
            {
                var producto = await _productoService.GetProductoByIdAsync(id);
                _logger.LogInformation("Producto encontrado. Id: {Id}", id);
                return Ok(new { message = "Producto encontrado", producto });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Producto no encontrado. Id: {Id}", id);
                return NotFound(new { message = "Producto no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar el producto. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al consultar el producto", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <param name="dto">Datos del producto a crear.</param>
        /// <returns>Producto creado.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CrearProductoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos requeridos faltantes al crear producto.");
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando crear producto: {Nombre}", dto.Nombre);
            try
            {
                var productoCreado = await _productoService.CreateProductoAsync(dto);
                _logger.LogInformation("Producto creado correctamente. Id: {Id}, Nombre: {Nombre}", productoCreado.Id, productoCreado.Nombre);
                return StatusCode(201, new { message = "Producto creado", producto = productoCreado });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al crear producto: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el producto.");
                return StatusCode(500, new { message = "Error al crear el producto", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="id">Identificador del producto a actualizar.</param>
        /// <param name="dto">Datos nuevos del producto.</param>
        /// <returns>Producto actualizado o error si no existe.</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] CrearProductoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos requeridos faltantes al actualizar producto. Id: {Id}", id);
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando actualizar producto. Id: {Id}, Nombre: {Nombre}", id, dto.Nombre);
            try
            {
                var productoActualizado = await _productoService.UpdateProductoAsync(id, dto);
                _logger.LogInformation("Producto actualizado correctamente. Id: {Id}", id);
                return Ok(new { message = "Producto actualizado", producto = productoActualizado });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Producto no encontrado al intentar actualizar. Id: {Id}", id);
                return NotFound(new { message = "Producto no encontrado" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al actualizar producto. Id: {Id}, Error: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el producto. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el producto", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un producto por su identificador.
        /// </summary>
        /// <param name="id">Identificador del producto a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Intentando eliminar producto. Id: {Id}", id);
            try
            {
                var eliminado = await _productoService.DeleteProductoAsync(id);
                if (!eliminado)
                {
                    _logger.LogWarning("Producto no encontrado al intentar eliminar. Id: {Id}", id);
                    return NotFound(new { message = "Producto no encontrado" });
                }
                _logger.LogInformation("Producto eliminado correctamente. Id: {Id}", id);
                return Ok(new { message = "Producto eliminado" });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Producto no encontrado al intentar eliminar. Id: {Id}", id);
                return NotFound(new { message = "Producto no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el producto. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar el producto", error = ex.Message });
            }
        }
    }
}