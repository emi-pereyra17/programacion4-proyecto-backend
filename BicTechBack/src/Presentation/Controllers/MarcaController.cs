using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BicTechBack.src.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de marcas de productos.
    /// </summary>
    [ApiController]
    [Route("marcas")]
    public class MarcaController : ControllerBase
    {
        private readonly IMarcaService _marcaService;
        private readonly ILogger<MarcaController> _logger;

        public MarcaController(IMarcaService marcaService, ILogger<MarcaController> logger)
        {
            _marcaService = marcaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de todas las marcas.
        /// </summary>
        /// <returns>Lista de marcas.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll()
        {
            _logger.LogInformation("Obteniendo lista de todas las marcas.");
            try
            {
                var marcas = await _marcaService.GetAllMarcasAsync();
                _logger.LogInformation("Marcas obtenidas correctamente. Total: {Total}", marcas.Count());
                return Ok(new { message = "Lista de Marcas", marcas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar las Marcas.");
                return StatusCode(500, new { message = "Error al consultar las Marcas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de marcas.
        /// </summary>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Cantidad de marcas por página (por defecto 10).</param>
        /// <param name="filtro">Filtro opcional por nombre.</param>
        /// <returns>Lista paginada de marcas y el total.</returns>
        [HttpGet("paginado")]
        public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? filtro = null)
        {
            _logger.LogInformation("Obteniendo marcas paginadas. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            try
            {
                var (marcas, total) = await _marcaService.GetMarcasAsync(page, pageSize, filtro);
                _logger.LogInformation("Marcas paginadas obtenidas correctamente. Total: {Total}", total);
                return Ok(new
                {
                    message = "Lista paginada de marcas",
                    total,
                    page,
                    pageSize,
                    marcas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar las marcas paginadas.");
                return StatusCode(500, new { message = "Error al consultar las marcas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una marca por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la marca.</param>
        /// <returns>Marca encontrada o error si no existe.</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById(int id)
        {
            _logger.LogInformation("Buscando marca por Id: {Id}", id);
            try
            {
                var marca = await _marcaService.GetMarcaByIdAsync(id);
                _logger.LogInformation("Marca encontrada. Id: {Id}", id);
                return Ok(new { message = "Marca encontrada", marca });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Marca no encontrada. Id: {Id}", id);
                return NotFound(new { message = "Marca no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar la Marca. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al consultar la Marca", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva marca.
        /// </summary>
        /// <param name="dto">Datos de la marca a crear.</param>
        /// <returns>Marca creada.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CrearMarcaDTO dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Nombre))
            {
                _logger.LogWarning("Datos requeridos faltantes al crear marca.");
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando crear marca: {Nombre}", dto.Nombre);
            try
            {
                var marcaCreada = await _marcaService.CreateMarcaAsync(dto);
                _logger.LogInformation("Marca creada correctamente. Id: {Id}, Nombre: {Nombre}", marcaCreada.Id, marcaCreada.Nombre);
                return StatusCode(201, new { message = "Marca creada", marca = marcaCreada });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al crear marca: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la Marca.");
                return StatusCode(500, new { message = "Error al crear la Marca", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una marca existente.
        /// </summary>
        /// <param name="id">Identificador de la marca a actualizar.</param>
        /// <param name="dto">Datos nuevos de la marca.</param>
        /// <returns>Marca actualizada o error si no existe.</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] CrearMarcaDTO dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Nombre))
            {
                _logger.LogWarning("Datos requeridos faltantes al actualizar marca. Id: {Id}", id);
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando actualizar marca. Id: {Id}, Nombre: {Nombre}", id, dto.Nombre);
            try
            {
                var marcaActualizada = await _marcaService.UpdateMarcaAsync(id, dto);
                _logger.LogInformation("Marca actualizada correctamente. Id: {Id}", id);
                return Ok(new { message = "Marca actualizada", marca = marcaActualizada });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Marca no encontrada al intentar actualizar. Id: {Id}", id);
                return NotFound(new { message = "Marca no encontrada" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al actualizar marca. Id: {Id}, Error: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la Marca. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar la Marca", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una marca por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la marca a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Intentando eliminar marca. Id: {Id}", id);
            try
            {
                var eliminado = await _marcaService.DeleteMarcaAsync(id);
                if (!eliminado)
                {
                    _logger.LogWarning("Marca no encontrada al intentar eliminar. Id: {Id}", id);
                    return NotFound(new { message = "Marca no encontrada" });
                }
                _logger.LogInformation("Marca eliminada correctamente. Id: {Id}", id);
                return Ok(new { message = "Marca eliminada" });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Marca no encontrada al intentar eliminar. Id: {Id}", id);
                return NotFound(new { message = "Marca no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la Marca. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar la Marca", error = ex.Message });
            }
        }
    }
}
