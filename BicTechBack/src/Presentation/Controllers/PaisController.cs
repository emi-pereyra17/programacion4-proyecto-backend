using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BicTechBack.src.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de paises de marcas.
    /// </summary>
    [ApiController]
    [Route("paises")]
    public class PaisController : ControllerBase
    {
        private readonly IPaisService _paisService;
        private readonly ILogger<PaisController> _logger;

        public PaisController(IPaisService paisService, ILogger<PaisController> logger)
        {
            _logger = logger;
            _paisService = paisService;
        }

        /// <summary>
        /// Obtiene la lista de todos los países.
        /// </summary>
        /// <returns>Lista de países.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll()
        {
            _logger.LogInformation("Obteniendo lista de todos los países.");
            try
            {
                var paises = await _paisService.GetAllPaisesAsync();
                _logger.LogInformation("Países obtenidos correctamente. Total: {Total}", paises.Count());
                return Ok(new { message = "Lista de Países", paises });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar los Países.");
                return StatusCode(500, new { message = "Error al consultar los Países", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de países.
        /// </summary>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Cantidad de países por página (por defecto 10).</param>
        /// <param name="filtro">Filtro opcional por nombre.</param>
        /// <returns>Lista paginada de países y el total.</returns>
        [HttpGet("paginado")]
        public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? filtro = null)
        {
            _logger.LogInformation("Obteniendo países paginados. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            try
            {
                var (paises, total) = await _paisService.GetPaisesAsync(page, pageSize, filtro);
                _logger.LogInformation("Países paginados obtenidos correctamente. Total: {Total}", total);
                return Ok(new
                {
                    message = "Lista paginada de países",
                    total,
                    page,
                    pageSize,
                    paises
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar los países paginados.");
                return StatusCode(500, new { message = "Error al consultar los países", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un país por su identificador.
        /// </summary>
        /// <param name="id">Identificador del país.</param>
        /// <returns>País encontrado o error si no existe.</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById(int id)
        {
            _logger.LogInformation("Buscando país por Id: {Id}", id);
            try
            {
                var pais = await _paisService.GetPaisByIdAsync(id);
                _logger.LogInformation("País encontrado. Id: {Id}", id);
                return Ok(new { message = "País encontrado", pais });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("País no encontrado. Id: {Id}", id);
                return NotFound(new { message = "País no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar el País. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al consultar el País", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo país.
        /// </summary>
        /// <param name="dto">Datos del país a crear.</param>
        /// <returns>País creado.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CrearPaisDTO dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Nombre))
            {
                _logger.LogWarning("Datos requeridos faltantes al crear país.");
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando crear país: {Nombre}", dto.Nombre);
            try
            {
                var paisCreado = await _paisService.CreatePaisAsync(dto);
                _logger.LogInformation("País creado correctamente. Id: {Id}, Nombre: {Nombre}", paisCreado.Id, paisCreado.Nombre);
                return StatusCode(201, new { message = "País creado", pais = paisCreado });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al crear país: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el País.");
                return StatusCode(500, new { message = "Error al crear el País", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un país existente.
        /// </summary>
        /// <param name="id">Identificador del país a actualizar.</param>
        /// <param name="dto">Datos nuevos del país.</param>
        /// <returns>País actualizado o error si no existe.</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] CrearPaisDTO dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Nombre))
            {
                _logger.LogWarning("Datos requeridos faltantes al actualizar país. Id: {Id}", id);
                return BadRequest(new { message = "Faltan datos requeridos" });
            }

            _logger.LogInformation("Intentando actualizar país. Id: {Id}, Nombre: {Nombre}", id, dto.Nombre);
            try
            {
                var paisActualizado = await _paisService.UpdatePaisAsync(id, dto);
                _logger.LogInformation("País actualizado correctamente. Id: {Id}", id);
                return Ok(new { message = "País actualizado", pais = paisActualizado });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("País no encontrado al intentar actualizar. Id: {Id}", id);
                return NotFound(new { message = "País no encontrado" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al actualizar país. Id: {Id}, Error: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el País. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el País", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un país por su identificador.
        /// </summary>
        /// <param name="id">Identificador del país a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Intentando eliminar país. Id: {Id}", id);
            try
            {
                var eliminado = await _paisService.DeletePaisAsync(id);
                if (!eliminado)
                {
                    _logger.LogWarning("País no encontrado al intentar eliminar. Id: {Id}", id);
                    return NotFound(new { message = "País no encontrado" });
                }
                _logger.LogInformation("País eliminado correctamente. Id: {Id}", id);
                return Ok(new { message = "País eliminado" });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("País no encontrado al intentar eliminar. Id: {Id}", id);
                return NotFound(new { message = "País no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el País. Id: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar el País", error = ex.Message });
            }
        }
    }
}