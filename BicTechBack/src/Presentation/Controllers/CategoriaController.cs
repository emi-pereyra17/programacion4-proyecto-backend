using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BicTechBack.src.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de categorías de productos.
    /// </summary>
    [ApiController]
    [Route("categorias")]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;
        private readonly ILogger<CategoriaController> _logger;

        public CategoriaController(ICategoriaService categoriaService, ILogger<CategoriaController> logger)
        {
            _categoriaService = categoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de todas las categorías.
        /// </summary>
        /// <returns>Lista de categorías.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll()
        {
            _logger.LogInformation("Obteniendo lista de todas las categorías.");
            try
            {
                var categorias = await _categoriaService.GetAllCategoriasAsync();
                _logger.LogInformation("Categorías obtenidas correctamente. Total: {Total}", categorias.Count());
                return Ok(new { message = "Lista de categorias", categorias });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar las categorías.");
                return StatusCode(500, new { message = "Error al consultar las categorias", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de categorías.
        /// </summary>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Cantidad de categorías por página (por defecto 10).</param>
        /// <param name="filtro">Filtro opcional por nombre.</param>
        /// <returns>Lista paginada de categorías y el total.</returns>
        [HttpGet("paginado")]
        public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? filtro = null)
        {
            _logger.LogInformation("Obteniendo categorías paginadas. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            try
            {
                var (categorias, total) = await _categoriaService.GetCategoriasAsync(page, pageSize, filtro);
                _logger.LogInformation("Categorías paginadas obtenidas correctamente. Total: {Total}", total);
                return Ok(new
                {
                    message = "Lista paginada de categorias",
                    total,
                    page,
                    pageSize,
                    categorias
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar las categorías paginadas.");
                return StatusCode(500, new { message = "Error al consultar las categorias", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una categoría por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la categoría.</param>
        /// <returns>Categoría encontrada o error si no existe.</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById(int id)
        {
            _logger.LogInformation("Buscando categoría por Id: {Id}", id);
            try
            {
                var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
                _logger.LogInformation("Categoría encontrada. Id: {Id}", id);
                return Ok(new { message = "Categoria encontrada", categoria });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Categoría no encontrada. Id: {Id}", id);
                return NotFound(new { message = "Categoria no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar la categoría. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al consultar la categoria", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva categoría.
        /// </summary>
        /// <param name="dto">Datos de la categoría a crear.</param>
        /// <returns>Categoría creada.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CrearCategoriaDTO dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Nombre))
            {
                _logger.LogWarning("Datos requeridos faltantes al crear categoría.");
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando crear categoría: {Nombre}", dto.Nombre);
            try
            {
                var categoriaCreada = await _categoriaService.CreateCategoriaAsync(dto);
                _logger.LogInformation("Categoría creada correctamente. Id: {Id}, Nombre: {Nombre}", categoriaCreada.Id, categoriaCreada.Nombre);
                return StatusCode(201, new { message = "Categoria creada", categoria = categoriaCreada });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al crear categoría: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la categoría.");
                return StatusCode(500, new { message = "Error al crear la categoria", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una categoría existente.
        /// </summary>
        /// <param name="id">Identificador de la categoría a actualizar.</param>
        /// <param name="dto">Datos nuevos de la categoría.</param>
        /// <returns>Categoría actualizada o error si no existe.</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] CrearCategoriaDTO dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Nombre))
            {
                _logger.LogWarning("Datos requeridos faltantes al actualizar categoría. Id: {Id}", id);
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando actualizar categoría. Id: {Id}, Nombre: {Nombre}", id, dto.Nombre);
            try
            {
                var categoriaActualizada = await _categoriaService.UpdateCategoriaAsync(id, dto);
                _logger.LogInformation("Categoría actualizada correctamente. Id: {Id}", id);
                return Ok(new { message = "Categoria actualizada", categoria = categoriaActualizada });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Categoría no encontrada al intentar actualizar. Id: {Id}", id);
                return NotFound(new { message = "Categoria no encontrada" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al actualizar categoría. Id: {Id}, Error: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la categoría. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar la categoria", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una categoría por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la categoría a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Intentando eliminar categoría. Id: {Id}", id);
            try
            {
                var eliminado = await _categoriaService.DeleteCategoriaAsync(id);
                if (!eliminado)
                {
                    _logger.LogWarning("Categoría no encontrada al intentar eliminar. Id: {Id}", id);
                    return NotFound(new { message = "Categoria no encontrada" });
                }
                _logger.LogInformation("Categoría eliminada correctamente. Id: {Id}", id);
                return Ok(new { message = "Categoria eliminada" });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Categoría no encontrada al intentar eliminar. Id: {Id}", id);
                return NotFound(new { message = "Categoria no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la categoría. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar la categoria", error = ex.Message });
            }
        }
    }
}