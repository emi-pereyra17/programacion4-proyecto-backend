using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BicTechBack.src.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de relaciones entre categorías y marcas.
    /// </summary>
    [ApiController]
    [Route("categoriaMarca")]
    public class CategoriaMarcaController : ControllerBase
    {
        private readonly ICategoriaMarcaService _categoriaMarcaService;
        private readonly ILogger<CategoriaMarcaController> _logger;

        public CategoriaMarcaController(ICategoriaMarcaService categoriaMarcaService, ILogger<CategoriaMarcaController> logger)
        {
            _categoriaMarcaService = categoriaMarcaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las relaciones categoría-marca.
        /// </summary>
        /// <returns>Lista de relaciones categoría-marca.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll()
        {
            _logger.LogInformation("Obteniendo todas las relaciones categoría-marca.");
            try
            {
                var relaciones = await _categoriaMarcaService.GetAllCMAsync();
                _logger.LogInformation("Relaciones obtenidas correctamente. Total: {Total}", relaciones.Count());
                return Ok(new { message = "Lista de relaciones", relaciones });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar las relaciones.");
                return StatusCode(500, new { message = "Error al consultar las relaciones", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de relaciones categoría-marca.
        /// </summary>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Cantidad de relaciones por página (por defecto 10).</param>
        /// <param name="filtro">Filtro opcional por categoría o marca.</param>
        /// <returns>Lista paginada de relaciones y el total.</returns>
        [HttpGet("paginado")]
        public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? filtro = null)
        {
            _logger.LogInformation("Obteniendo relaciones paginadas. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            try
            {
                var (cms, total) = await _categoriaMarcaService.GetCMAsync(page, pageSize, filtro);
                _logger.LogInformation("Relaciones paginadas obtenidas correctamente. Total: {Total}", total);
                return Ok(new
                {
                    message = "Lista paginada de cms",
                    total,
                    page,
                    pageSize,
                    cms
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar las cms paginadas.");
                return StatusCode(500, new { message = "Error al consultar las cms", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva relación entre una categoría y una marca.
        /// </summary>
        /// <param name="dto">Datos de la relación a crear.</param>
        /// <returns>Relación creada.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CrearCategoriaMarcaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos requeridos faltantes al crear relación categoría-marca.");
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando crear relación categoría-marca. CategoriaId: {CategoriaId}, MarcaId: {MarcaId}", dto.CategoriaId, dto.MarcaId);
            try
            {
                var relacionCreada = await _categoriaMarcaService.CreateCMAsync(dto);
                _logger.LogInformation("Relación creada correctamente. Id: {Id}", relacionCreada.Id);
                return StatusCode(201, new { message = "Relacion creada", relacion = relacionCreada });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al crear relación: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Entidad no encontrada al crear relación: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la relación.");
                return StatusCode(500, new { message = "Error al crear la relacion", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las marcas asociadas a una categoría.
        /// </summary>
        /// <param name="id">Identificador de la categoría.</param>
        /// <returns>Lista de marcas asociadas a la categoría.</returns>
        [HttpGet("categoria/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> ObtenerMarcasPorCategoria(int id)
        {
            _logger.LogInformation("Obteniendo marcas para la categoría. CategoriaId: {CategoriaId}", id);
            try
            {
                var marcas = await _categoriaMarcaService.GetMarcasPorCategoriaAsync(id);
                if (marcas == null || !marcas.Any())
                {
                    _logger.LogWarning("Categoría no encontrada o sin marcas. CategoriaId: {CategoriaId}", id);
                    return NotFound(new { message = "Categoria no encontrada" });
                }

                _logger.LogInformation("Marcas obtenidas para la categoría. CategoriaId: {CategoriaId}", id);
                return Ok(new { message = $"Marcas de la categoria {id}", marcas });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Categoría no encontrada al buscar marcas. CategoriaId: {CategoriaId}", id);
                return NotFound(new { message = "Categoria no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las marcas por categoría. CategoriaId: {CategoriaId}", id);
                return StatusCode(500, new { message = "Error al obtener las marcas por categoria", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las categorías asociadas a una marca.
        /// </summary>
        /// <param name="id">Identificador de la marca.</param>
        /// <returns>Lista de categorías asociadas a la marca.</returns>
        [HttpGet("marca/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> ObtenerCategoriasPorMarca(int id)
        {
            _logger.LogInformation("Obteniendo categorías para la marca. MarcaId: {MarcaId}", id);
            try
            {
                var categorias = await _categoriaMarcaService.GetCategoriasPorMarcaAsync(id);
                if (categorias == null || !categorias.Any())
                {
                    _logger.LogWarning("Marca no encontrada o sin categorías. MarcaId: {MarcaId}", id);
                    return NotFound(new { message = "Marca no encontrada" });
                }

                _logger.LogInformation("Categorías obtenidas para la marca. MarcaId: {MarcaId}", id);
                return Ok(new { message = $"Categorias de la marca {id}", categorias });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Marca no encontrada al buscar categorías. MarcaId: {MarcaId}", id);
                return NotFound(new { message = "Marca no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar las categorías por marca. MarcaId: {MarcaId}", id);
                return StatusCode(500, new { message = "Error al consultar las categorias por marca", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una relación categoría-marca por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la relación a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Intentando eliminar relación categoría-marca. Id: {Id}", id);
            try
            {
                var eliminado = await _categoriaMarcaService.DeleteCMAsync(id);
                if (!eliminado)
                {
                    _logger.LogWarning("Relación no encontrada al intentar eliminar. Id: {Id}", id);
                    return NotFound(new { message = "Relacion no encontrada" });
                }
                _logger.LogInformation("Relación eliminada correctamente. Id: {Id}", id);
                return Ok(new { message = "Relacion eliminada" });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Relación no encontrada al intentar eliminar. Id: {Id}", id);
                return NotFound(new { message = "Relacion no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la relación. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar la relacion", error = ex.Message });
            }
        }
    }
}
