using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
namespace BicTechBack.src.Core.Services
{
    public class CategoriaMarcaService : ICategoriaMarcaService
    {
        private readonly ICategoriaMarcaRepository _repository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IMarcaRepository _marcaRepository;
        private readonly IMapper _mapper;
        private readonly IAppLogger<CategoriaMarcaService> _logger;

        public CategoriaMarcaService(
            ICategoriaMarcaRepository categoriaMarcaRepository,
            ICategoriaRepository categoriaRepository,
            IMarcaRepository marcaRepository,
            IMapper mapper,
            IAppLogger<CategoriaMarcaService> logger)
        {
            _repository = categoriaMarcaRepository;
            _categoriaRepository = categoriaRepository;
            _marcaRepository = marcaRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CategoriaMarcaDTO> CreateCMAsync(CrearCategoriaMarcaDTO dto)
        {
            _logger.LogInformation("Intentando crear relación categoría-marca: CategoriaId={CategoriaId}, MarcaId={MarcaId}", dto.CategoriaId, dto.MarcaId);

            var cms = await _repository.GetAllAsync();
            var categoria = await _categoriaRepository.GetByIdAsync(dto.CategoriaId);
            if (categoria == null)
            {
                _logger.LogWarning("Categoría no encontrada al crear relación. CategoriaId={CategoriaId}", dto.CategoriaId);
                throw new KeyNotFoundException("Categoría no encontrada.");
            }
            var marca = await _marcaRepository.GetByIdAsync(dto.MarcaId);
            if (marca == null)
            {
                _logger.LogWarning("Marca no encontrada al crear relación. MarcaId={MarcaId}", dto.MarcaId);
                throw new KeyNotFoundException("Marca no encontrada.");
            }
            if (cms.Any(cm => cm.CategoriaId == dto.CategoriaId && cm.MarcaId == dto.MarcaId))
            {
                _logger.LogWarning("Intento de crear relación duplicada entre categoría y marca. CategoriaId={CategoriaId}, MarcaId={MarcaId}", dto.CategoriaId, dto.MarcaId);
                throw new InvalidOperationException("Ya existe una relación entre esta categoría y marca.");
            }

            var cm = _mapper.Map<CategoriaMarca>(dto);
            var cmCreada = await _repository.AddAsync(cm);

            _logger.LogInformation("Relación categoría-marca creada correctamente. Id={Id}, CategoriaId={CategoriaId}, MarcaId={MarcaId}", cmCreada.Id, cmCreada.CategoriaId, cmCreada.MarcaId);

            return _mapper.Map<CategoriaMarcaDTO>(cmCreada);
        }

        public async Task<bool> DeleteCMAsync(int id)
        {
            _logger.LogInformation("Intentando eliminar relación categoría-marca. Id={Id}", id);

            var cm = await _repository.DeleteAsync(id);
            if (!cm)
            {
                _logger.LogWarning("Relación categoría-marca no encontrada al intentar eliminar. Id={Id}", id);
                throw new KeyNotFoundException("Relación categoría-marca no encontrada.");
            }

            _logger.LogInformation("Relación categoría-marca eliminada correctamente. Id={Id}", id);
            return true;
        }

        public async Task<IEnumerable<CategoriaMarcaDTO>> GetAllCMAsync()
        {
            _logger.LogInformation("Obteniendo todas las relaciones categoría-marca.");
            var cms = await _repository.GetAllAsync();
            if (cms == null || !cms.Any())
            {
                _logger.LogInformation("No se encontraron relaciones categoría-marca en la base de datos.");
                return Enumerable.Empty<CategoriaMarcaDTO>();
            }
            return _mapper.Map<IEnumerable<CategoriaMarcaDTO>>(cms);
        }

        public async Task<IEnumerable<MarcaDTO>> GetMarcasPorCategoriaAsync(int categoriaId)
        {
            _logger.LogInformation("Obteniendo marcas para la categoría. CategoriaId={CategoriaId}", categoriaId);

            var categoria = await _categoriaRepository.GetByIdAsync(categoriaId);
            if (categoria == null)
            {
                _logger.LogWarning("Categoría no encontrada al buscar marcas. CategoriaId={CategoriaId}", categoriaId);
                throw new KeyNotFoundException("Categoria no encontrada.");
            }

            var relaciones = await _repository.GetByCategoriaIdAsync(categoriaId);
            var marcas = relaciones.Select(r => r.Marca).Distinct().ToList();

            return _mapper.Map<IEnumerable<MarcaDTO>>(marcas);
        }

        public async Task<IEnumerable<CategoriaDTO>> GetCategoriasPorMarcaAsync(int marcaId)
        {
            _logger.LogInformation("Obteniendo categorías para la marca. MarcaId={MarcaId}", marcaId);

            var marca = await _marcaRepository.GetByIdAsync(marcaId);
            if (marca == null)
            {
                _logger.LogWarning("Marca no encontrada al buscar categorías. MarcaId={MarcaId}", marcaId);
                throw new KeyNotFoundException("Marca no encontrada.");
            }

            var relaciones = await _repository.GetByMarcaIdAsync(marcaId);
            var categorias = relaciones.Select(r => r.Categoria).Distinct().ToList();

            return _mapper.Map<IEnumerable<CategoriaDTO>>(categorias);
        }

        public async Task<IEnumerable<CategoriaMarcaDTO>> GetCMByCategoriaIdAsync(int categoriaId)
        {
            _logger.LogInformation("Obteniendo relaciones por categoría. CategoriaId={CategoriaId}", categoriaId);

            var categoria = await _categoriaRepository.GetByIdAsync(categoriaId);
            if (categoria == null)
            {
                _logger.LogWarning("Categoría no encontrada al buscar relaciones. CategoriaId={CategoriaId}", categoriaId);
                throw new KeyNotFoundException("Categoría no encontrada.");
            }

            var cms = await _repository.GetByCategoriaIdAsync(categoriaId);
            if (cms == null || !cms.Any())
            {
                _logger.LogInformation("No se encontraron relaciones para la categoría. CategoriaId={CategoriaId}", categoriaId);
                return Enumerable.Empty<CategoriaMarcaDTO>();
            }

            return _mapper.Map<IEnumerable<CategoriaMarcaDTO>>(cms);
        }

        public async Task<IEnumerable<CategoriaMarcaDTO>> GetCMByMarcaIdAsync(int marcaId)
        {
            _logger.LogInformation("Obteniendo relaciones por marca. MarcaId={MarcaId}", marcaId);

            var marca = await _marcaRepository.GetByIdAsync(marcaId);
            if (marca == null)
            {
                _logger.LogWarning("Marca no encontrada al buscar relaciones. MarcaId={MarcaId}", marcaId);
                throw new KeyNotFoundException("Marca no encontrada.");
            }

            var cms = await _repository.GetByMarcaIdAsync(marcaId);
            if (cms == null || !cms.Any())
            {
                _logger.LogInformation("No se encontraron relaciones para la marca. MarcaId={MarcaId}", marcaId);
                return Enumerable.Empty<CategoriaMarcaDTO>();
            }

            return _mapper.Map<IEnumerable<CategoriaMarcaDTO>>(cms);
        }

        public async Task<(IEnumerable<CategoriaMarcaDTO> CategoriasMarcas, int Total)> GetCMAsync(int page, int pageSize, string? filtro)
        {
            _logger.LogInformation("Obteniendo relaciones categoría-marca paginadas. Página={Page}, Tamaño={PageSize}, Filtro={Filtro}", page, pageSize, filtro);

            var cms = await _repository.GetAllAsync();

            var total = cms.Count();

            var cmsPaginados = cms
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var cmsDto = _mapper.Map<IEnumerable<CategoriaMarcaDTO>>(cmsPaginados);

            return (cmsDto, total);
        }
    }
}