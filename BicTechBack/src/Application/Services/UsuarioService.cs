using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;

namespace BicTechBack.src.Core.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAppLogger<UsuarioService> _logger;
        private readonly IPasswordHasherService _passwordHasher;


        public UsuarioService(
            IUsuarioRepository repository,
            IMapper mapper,
            IAppLogger<UsuarioService> logger,
            IPasswordHasherService passwordHasher)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        public async Task<UsuarioDTO> CreateUsuarioAsync(CrearUsuarioDTO dto, string rol)
        {
            _logger.LogInformation("Intentando crear usuario con Email: {Email}", dto.Email);

            var usuarios = await _repository.GetAllAsync();
            if (usuarios.Any(u => u.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Intento de crear Usuario con Email ya existente: {Email}", dto.Email);
                throw new InvalidOperationException("Ya existe un usuario con ese email.");
            }

            if (string.IsNullOrWhiteSpace(rol))
            {
                _logger.LogWarning("Rol vacío o nulo al crear usuario con email: {Email}", dto.Email);
                throw new ArgumentException("El rol no puede ser nulo o vacío.", nameof(rol));
            }

            var usuario = _mapper.Map<Usuario>(dto);

            if (!Enum.TryParse<RolUsuario>(rol, true, out var rolUsuario))
            {
                _logger.LogWarning("Rol inválido '{Rol}' al crear usuario con email: {Email}", rol, dto.Email);
                throw new ArgumentException("El rol especificado no es válido.", nameof(rol));
            }

            usuario.Rol = rolUsuario;

            usuario.Password = _passwordHasher.HashPassword(dto.Password);

            var usuarioCreadoId = await _repository.CreateAsync(usuario);
            usuario.Id = usuarioCreadoId;

            _logger.LogInformation("Usuario creado correctamente. Id: {Id}, Email: {Email}", usuario.Id, usuario.Email);
            return _mapper.Map<UsuarioDTO>(usuario);
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            _logger.LogInformation("Intentando eliminar usuario con Id: {Id}", id);
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null)
            {
                _logger.LogWarning("Intento de eliminar usuario no existente. Id: {Id}", id);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            var eliminado = await _repository.DeleteAsync(id);

            if (eliminado)
                _logger.LogInformation("Usuario eliminado correctamente. Id: {Id}", id);
            else
                _logger.LogWarning("No se pudo eliminar el usuario. Id: {Id}", id);

            return eliminado;
        }

        public async Task<IEnumerable<UsuarioDTO>> GetAllUsuariosAsync()
        {
            _logger.LogInformation("Obteniendo todos los usuarios.");
            var usuarios = await _repository.GetAllAsync();
            if (usuarios == null || !usuarios.Any())
            {
                _logger.LogInformation("No se encontraron usuarios en la base de datos.");
                return Enumerable.Empty<UsuarioDTO>();
            }
            return _mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);
        }

        public async Task<UsuarioDTO> GetUsuarioByIdAsync(int id)
        {
            _logger.LogInformation("Buscando usuario por Id: {Id}", id);
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado. Id: {Id}", id);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            return _mapper.Map<UsuarioDTO>(usuario);
        }

        public async Task<(IEnumerable<UsuarioDTO> Usuarios, int Total)> GetUsuariosAsync(int page, int pageSize, string? filtro)
        {
            _logger.LogInformation("Obteniendo usuarios paginados. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            var usuarios = await _repository.GetAllAsync();

            var total = usuarios.Count();

            var usuariosPaginados = usuarios
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var usuariosDto = _mapper.Map<IEnumerable<UsuarioDTO>>(usuariosPaginados);

            return (usuariosDto, total);
        }

        public async Task<UsuarioDTO> UpdateUsuarioAsync(CrearUsuarioDTO dto, int id)
        {
            _logger.LogInformation("Intentando actualizar usuario. Id: {Id}, Email: {Email}", id, dto.Email);
            var usuarioExistente = await _repository.GetByIdAsync(id);
            if (usuarioExistente == null)
            {
                _logger.LogWarning("Usuario no encontrado al intentar actualizar. Id: {Id}", id);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            var usuarios = await _repository.GetAllAsync();
            if (usuarios.Any(u => u.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase) && u.Id != id))
            {
                _logger.LogWarning("Intento de actualizar usuario con email ya existente: {Email}", dto.Email);
                throw new InvalidOperationException("Ya existe un usuario con ese email.");
            }

            usuarioExistente.Nombre = dto.Nombre;
            usuarioExistente.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                usuarioExistente.Password = _passwordHasher.HashPassword(dto.Password);
            }

            var usuarioActualizado = await _repository.UpdateAsync(usuarioExistente);
            _logger.LogInformation("Usuario actualizado correctamente. Id: {Id}, Email: {Email}", usuarioActualizado.Id, usuarioActualizado.Email);
            return _mapper.Map<UsuarioDTO>(usuarioActualizado);
        }
    }
}