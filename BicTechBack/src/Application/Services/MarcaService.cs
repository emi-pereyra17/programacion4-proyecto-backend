using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;

namespace BicTechBack.src.Core.Services
{
    public class MarcaService : IMarcaService
    {
        private readonly IMarcaRepository _repository;
        private readonly IPaisRepository _paisRepository;
        private readonly IMapper _mapper;
        private readonly IAppLogger<MarcaService> _logger;

        public MarcaService(IMarcaRepository repository, IMapper mapper, IAppLogger<MarcaService> logger, IPaisRepository paisRepository)
        {
            _repository = repository;
            _paisRepository = paisRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MarcaDTO> CreateMarcaAsync(CrearMarcaDTO dto)
        {
            _logger.LogInformation("Intentando crear marca: {Nombre}", dto.Nombre);
            var pais = await _paisRepository.GetByIdAsync(dto.PaisId);
            if (pais == null)
            {
                _logger.LogWarning("País no encontrado al intentar crear marca. Id: {PaisId}", dto.PaisId);
                throw new KeyNotFoundException("País no encontrado.");
            }

            if (await _repository.ExistsByNameAsync(dto.Nombre))
            {
                _logger.LogWarning("Intento de crear marca con nombre duplicado: {Nombre}", dto.Nombre);
                throw new InvalidOperationException("Ya existe una marca con ese nombre.");
            }

            var marca = _mapper.Map<Marca>(dto);
            var marcaCreada = await _repository.AddAsync(marca);

            _logger.LogInformation("Marca creada correctamente. Id: {Id}, Nombre: {Nombre}", marcaCreada.Id, marcaCreada.Nombre);

            return _mapper.Map<MarcaDTO>(marcaCreada);
        }

        public async Task<bool> DeleteMarcaAsync(int id)
        {
            _logger.LogInformation("Intentando eliminar marca. Id: {Id}", id);

            var eliminado = await _repository.DeleteAsync(id);
            if (!eliminado)
            {
                _logger.LogWarning("Marca no encontrada al intentar eliminar. Id: {Id}", id);
                throw new KeyNotFoundException("Marca no encontrada.");
            }

            _logger.LogInformation("Marca eliminada correctamente. Id: {Id}", id);
            return true;
        }

        public async Task<IEnumerable<MarcaDTO>> GetAllMarcasAsync()
        {
            _logger.LogInformation("Obteniendo todas las marcas.");
            var marcas = await _repository.GetAllAsync();
            if (marcas == null || !marcas.Any())
            {
                _logger.LogInformation("No se encontraron marcas en la base de datos.");
                return Enumerable.Empty<MarcaDTO>();
            }

            return _mapper.Map<IEnumerable<MarcaDTO>>(marcas);
        }

        public async Task<MarcaDTO> GetMarcaByIdAsync(int id)
        {
            _logger.LogInformation("Buscando marca por Id: {Id}", id);
            var marca = await _repository.GetByIdAsync(id);
            if (marca == null)
            {
                _logger.LogWarning("Marca no encontrada. Id: {Id}", id);
                throw new KeyNotFoundException("Marca no encontrada.");
            }
            return _mapper.Map<MarcaDTO>(marca);
        }

        public async Task<(IEnumerable<MarcaDTO> Marcas, int Total)> GetMarcasAsync(int page, int pageSize, string? filtro)
        {
            _logger.LogInformation("Obteniendo marcas paginadas. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            var marcas = await _repository.GetAllAsync();

            var total = marcas.Count();

            var marcasPaginados = marcas
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var marcasDto = _mapper.Map<IEnumerable<MarcaDTO>>(marcasPaginados);

            return (marcasDto, total);
        }

        public async Task<MarcaDTO> UpdateMarcaAsync(int id, CrearMarcaDTO dto)
        {
            _logger.LogInformation("Intentando actualizar marca. Id: {Id}, Nombre: {Nombre}", id, dto.Nombre);

            var marcaExistente = await _repository.GetByIdAsync(id);
            var pais = await _paisRepository.GetByIdAsync(dto.PaisId);

            if (marcaExistente == null)
            {
                _logger.LogWarning("Marca no encontrada al intentar actualizar. Id: {Id}", id);
                throw new KeyNotFoundException("Marca no encontrada.");
            }

            if (pais == null)
            {
                _logger.LogWarning("País no encontrado al intentar crear marca. Id: {PaisId}", dto.PaisId);
                throw new KeyNotFoundException("País no encontrado.");
            }

            if (await _repository.ExistsByNameAsync(dto.Nombre, id))
            {
                _logger.LogWarning("Intento de actualizar marca con nombre duplicado: {Nombre}", dto.Nombre);
                throw new InvalidOperationException("Ya existe una marca con ese nombre.");
            }


            _mapper.Map(dto, marcaExistente);

            var marcaActualizada = await _repository.UpdateAsync(marcaExistente);

            _logger.LogInformation("Marca actualizada correctamente. Id: {Id}, Nombre: {Nombre}", marcaActualizada.Id, marcaActualizada.Nombre);

            return _mapper.Map<MarcaDTO>(marcaActualizada);
        }
    }
}