using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Entities;
using AutoMapper;

namespace BicTechBack.src.Core.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAppLogger<CategoriaService> _logger;

        public CategoriaService(ICategoriaRepository repository, IMapper mapper, IAppLogger<CategoriaService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CategoriaDTO> CreateCategoriaAsync(CrearCategoriaDTO dto)
        {
            _logger.LogInformation("Intentando crear categoría: {Nombre}", dto.Nombre);

            var categorias = await _repository.GetAllAsync();
            if (categorias.Any(c => c.Nombre.Equals(dto.Nombre, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Intento de crear categoría con nombre duplicado: {Nombre}", dto.Nombre);
                throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
            }

            var categoria = _mapper.Map<Categoria>(dto);
            var categoriaCreada = await _repository.AddAsync(categoria);

            _logger.LogInformation("Categoría creada correctamente. Id: {Id}, Nombre: {Nombre}", categoriaCreada.Id, categoriaCreada.Nombre);

            return _mapper.Map<CategoriaDTO>(categoriaCreada);
        }

        public async Task<bool> DeleteCategoriaAsync(int id)
        {
            _logger.LogInformation("Intentando eliminar categoría. Id: {Id}", id);

            var eliminado = await _repository.DeleteAsync(id);
            if (!eliminado)
            {
                _logger.LogWarning("Categoría no encontrada al intentar eliminar. Id: {Id}", id);
                throw new KeyNotFoundException("Categoría no encontrada.");
            }

            _logger.LogInformation("Categoría eliminada correctamente. Id: {Id}", id);
            return true;
        }

        public async Task<IEnumerable<CategoriaDTO>> GetAllCategoriasAsync()
        {
            _logger.LogInformation("Obteniendo todas las categorías.");
            var categorias = await _repository.GetAllAsync();
            if (categorias == null || !categorias.Any())
            {
                _logger.LogInformation("No se encontraron categorías en la base de datos.");
                return Enumerable.Empty<CategoriaDTO>();
            }
            return _mapper.Map<IEnumerable<CategoriaDTO>>(categorias);
        }

        public async Task<CategoriaDTO> GetCategoriaByIdAsync(int id)
        {
            _logger.LogInformation("Buscando categoría por Id: {Id}", id);
            var categoria = await _repository.GetByIdAsync(id);
            if (categoria == null)
            {
                _logger.LogWarning("Categoría no encontrada. Id: {Id}", id);
                throw new KeyNotFoundException("Categoría no encontrada.");
            }
            return _mapper.Map<CategoriaDTO>(categoria);
        }

        public async Task<(IEnumerable<CategoriaDTO> Categorias, int Total)> GetCategoriasAsync(int page, int pageSize, string? filtro)
        {
            _logger.LogInformation("Obteniendo categorías paginadas. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            var categorias = await _repository.GetAllAsync();

            var total = categorias.Count();

            var categoriasPaginadas = categorias
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var categoriasDto = _mapper.Map<IEnumerable<CategoriaDTO>>(categoriasPaginadas);

            return (categoriasDto, total);
        }

        public async Task<CategoriaDTO> UpdateCategoriaAsync(int id, CrearCategoriaDTO dto)
        {
            _logger.LogInformation("Intentando actualizar categoría. Id: {Id}, Nombre: {Nombre}", id, dto.Nombre);

            var categoriaExistente = await _repository.GetByIdAsync(id);
            if (categoriaExistente == null)
            {
                _logger.LogWarning("Categoría no encontrada al intentar actualizar. Id: {Id}", id);
                throw new KeyNotFoundException("Categoría no encontrada.");
            }
            var categorias = await _repository.GetAllAsync();
            if (categorias.Any(c => c.Nombre.Equals(dto.Nombre, StringComparison.OrdinalIgnoreCase) && c.Id != id))
            {
                _logger.LogWarning("Intento de actualizar categoría con nombre duplicado: {Nombre}", dto.Nombre);
                throw new InvalidOperationException("Ya existe una categoria con ese nombre.");
            }

            _mapper.Map(dto, categoriaExistente);

            var categoriaActualizada = await _repository.UpdateAsync(categoriaExistente);

            _logger.LogInformation("Categoría actualizada correctamente. Id: {Id}, Nombre: {Nombre}", categoriaActualizada.Id, categoriaActualizada.Nombre);

            return _mapper.Map<CategoriaDTO>(categoriaActualizada);
        }
    }
}