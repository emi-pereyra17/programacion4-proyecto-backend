using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;

namespace BicTechBack.src.Core.Services
{
    public class PaisService : IPaisService
    {
        private readonly IPaisRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAppLogger<PaisService> _logger;

        public PaisService(IPaisRepository repository, IMapper mapper, IAppLogger<PaisService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaisDTO> CreatePaisAsync(CrearPaisDTO dto)
        {
            _logger.LogInformation("Intentando crear país: {Nombre}", dto.Nombre);

            var paises = await _repository.GetAllAsync();
            if (paises.Any(p => p.Nombre.Equals(dto.Nombre, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Intento de crear país con nombre duplicado: {Nombre}", dto.Nombre);
                throw new InvalidOperationException("Ya existe un país con ese nombre.");
            }
            var pais = _mapper.Map<Pais>(dto);
            var paisCreado = await _repository.AddAsync(pais);

            _logger.LogInformation("País creado correctamente. Id: {Id}, Nombre: {Nombre}", paisCreado.Id, paisCreado.Nombre);
            return _mapper.Map<PaisDTO>(paisCreado);
        }

        public async Task<bool> DeletePaisAsync(int id)
        {
            _logger.LogInformation("Intentando eliminar país. Id: {Id}", id);
            var eliminado = await _repository.DeleteAsync(id);
            if (!eliminado)
            {
                _logger.LogWarning("País no encontrado al intentar eliminar. Id: {Id}", id);
                throw new KeyNotFoundException("Pais no encontrado.");
            }
            _logger.LogInformation("País eliminado correctamente. Id: {Id}", id);
            return true;
        }

        public async Task<IEnumerable<PaisDTO>> GetAllPaisesAsync()
        {
            _logger.LogInformation("Obteniendo todos los países.");
            var paises = await _repository.GetAllAsync();
            if (paises == null || !paises.Any())
            {
                _logger.LogWarning("No se encontraron países.");
                return Enumerable.Empty<PaisDTO>();
            }

            return _mapper.Map<IEnumerable<PaisDTO>>(paises);
        }

        public async Task<PaisDTO> GetPaisByIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo país por ID: {Id}", id);
            var pais = await _repository.GetByIdAsync(id);
            if (pais == null)
            {
                _logger.LogWarning("País no encontrado. Id: {Id}", id);
                throw new KeyNotFoundException("Pais no encontrado.");
            }
            return _mapper.Map<PaisDTO>(pais);
        }

        public async Task<(IEnumerable<PaisDTO> Paises, int Total)> GetPaisesAsync(int page, int pageSize, string? filtro)
        {
            _logger.LogInformation("Obteniendo países con paginación. Página: {Page}, Tamaño de página: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            var paises = await _repository.GetAllAsync();

            var total = paises.Count();

            var paisesPaginados = paises
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var paisesDto = _mapper.Map<IEnumerable<PaisDTO>>(paisesPaginados);
            return (paisesDto, total);
        }

        public async Task<PaisDTO> UpdatePaisAsync(int id, CrearPaisDTO dto)
        {
            _logger.LogInformation("Intentando actualizar país. Id: {Id}, Nombre: {Nombre}", id, dto.Nombre);
            var paisExistente = await _repository.GetByIdAsync(id);
            var paises = await _repository.GetAllAsync();

            if (paisExistente == null)
            {
                _logger.LogWarning("País no encontrado al intentar actualizar. Id: {Id}", id);
                throw new KeyNotFoundException("Pais no encontrado.");
            }
            if (paises.Any(p => p.Nombre.Equals(dto.Nombre, StringComparison.OrdinalIgnoreCase) && p.Id != id))
            {
                _logger.LogWarning("Intento de actualizar país con nombre duplicado: {Nombre}", dto.Nombre);
                throw new InvalidOperationException("Ya existe un país con ese nombre.");
            }

            _mapper.Map(dto, paisExistente);
            var paisActualizado = await _repository.UpdateAsync(paisExistente);
            _logger.LogInformation("País actualizado correctamente. Id: {Id}, Nombre: {Nombre}", paisActualizado.Id, paisActualizado.Nombre);
            return _mapper.Map<PaisDTO>(paisActualizado);
        }
    }
}