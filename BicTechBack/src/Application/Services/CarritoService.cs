using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BicTechBack.src.Core.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly ICarritoRepository _repository;
        private readonly IProductoRepository _productoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CarritoService> _logger; 

        public CarritoService(
            ICarritoRepository repository,
            IMapper mapper,
            IUsuarioRepository usuarioRepository,
            IProductoRepository productoRepository,
            ILogger<CarritoService> logger) 
        {
            _repository = repository;
            _mapper = mapper;
            _usuarioRepository = usuarioRepository;
            _productoRepository = productoRepository;
            _logger = logger;
        }

        public async Task<CarritoDTO> AddProductoToCarritoAsync(int usuarioId, int productoId, int cantidad)
        {
            _logger.LogInformation("Agregando producto {ProductoId} al carrito del usuario {UsuarioId}", productoId, usuarioId);

            if (cantidad <= 0)
            {
                _logger.LogWarning("Cantidad inválida: {Cantidad} para el usuario {UsuarioId}", cantidad, usuarioId);
                throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
            }

            var producto = await _productoRepository.GetByIdAsync(productoId);
            if (producto == null)
            {
                _logger.LogWarning("Producto no encontrado: {ProductoId}", productoId);
                throw new InvalidOperationException("El producto no existe.");
            }

            var carrito = await _repository.GetByUsuarioIdAsync(usuarioId);
            var cantidadActual = carrito?.CarritosDetalles?.FirstOrDefault(cd => cd.ProductoId == productoId)?.Cantidad ?? 0;
            var cantidadTotal = cantidadActual + cantidad;

            if (producto.Stock < cantidadTotal)
            {
                _logger.LogWarning("Stock insuficiente para producto {ProductoId}. Stock disponible: {Stock}, Solicitado: {CantidadTotal}", productoId, producto.Stock, cantidadTotal);
                throw new InvalidOperationException("No hay suficiente stock disponible.");
            }

            var carritoActualizado = await _repository.AddProductoAsync(usuarioId, productoId, cantidad);
            if (carritoActualizado == null)
            {
                _logger.LogError("No se pudo agregar el producto {ProductoId} al carrito del usuario {UsuarioId}", productoId, usuarioId);
                throw new InvalidOperationException("No se pudo agregar el producto al carrito.");
            }

            _logger.LogInformation("Producto {ProductoId} agregado correctamente al carrito del usuario {UsuarioId}", productoId, usuarioId);
            return _mapper.Map<CarritoDTO>(carritoActualizado);
        }

        public async Task<CarritoDTO> ClearCarritoAsync(int usuarioId)
        {
            _logger.LogInformation("Limpiando carrito del usuario {UsuarioId}", usuarioId);

            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado al limpiar carrito. UsuarioId: {UsuarioId}", usuarioId);
                throw new InvalidOperationException("El usuario especificado no existe.");
            }

            var carrito = await _repository.ClearAsync(usuarioId);
            if (carrito == null)
            {
                _logger.LogError("No se pudo limpiar el carrito del usuario {UsuarioId}", usuarioId);
                throw new InvalidOperationException("No se pudo limpiar el carrito del usuario.");
            }

            _logger.LogInformation("Carrito del usuario {UsuarioId} limpiado correctamente", usuarioId);
            return _mapper.Map<CarritoDTO>(carrito);
        }

        public async Task<CarritoDTO> DeleteProductoFromCarritoAsync(int usuarioId, int productoId)
        {
            _logger.LogInformation("Eliminando producto {ProductoId} del carrito del usuario {UsuarioId}", productoId, usuarioId);

            var carrito = await _repository.GetByUsuarioIdAsync(usuarioId);
            if (carrito == null)
            {
                _logger.LogWarning("Carrito no encontrado para el usuario {UsuarioId}", usuarioId);
                throw new InvalidOperationException("El carrito del usuario especificado no existe.");
            }

            var detalle = carrito.CarritosDetalles?.FirstOrDefault(cd => cd.ProductoId == productoId);
            if (detalle == null)
            {
                _logger.LogWarning("Producto {ProductoId} no está en el carrito del usuario {UsuarioId}", productoId, usuarioId);
                throw new InvalidOperationException("El producto especificado no está en el carrito.");
            }

            var carritoActualizado = await _repository.DeleteAsync(usuarioId, productoId);
            if (carritoActualizado == null)
            {
                _logger.LogError("No se pudo eliminar el producto {ProductoId} del carrito del usuario {UsuarioId}", productoId, usuarioId);
                throw new InvalidOperationException("No se pudo eliminar el producto del carrito.");
            }

            _logger.LogInformation("Producto {ProductoId} eliminado correctamente del carrito del usuario {UsuarioId}", productoId, usuarioId);
            return _mapper.Map<CarritoDTO>(carritoActualizado);
        }

        public async Task<IEnumerable<CarritoDTO>> GetAllCarritosAsync()
        {
            _logger.LogInformation("Obteniendo todos los carritos.");
            var carritos = await _repository.GetAllAsync();
            if (carritos == null || !carritos.Any())
            {
                _logger.LogInformation("No se encontraron carritos en la base de datos.");
                return Enumerable.Empty<CarritoDTO>();
            }
            return _mapper.Map<IEnumerable<CarritoDTO>>(carritos);
        }

        public async Task<CarritoDTO> GetCarritoByUsuarioIdAsync(int usuarioId)
        {
            _logger.LogInformation("Obteniendo carrito para el usuario {UsuarioId}", usuarioId);
            var carrito = await _repository.GetByUsuarioIdAsync(usuarioId);
            if (carrito == null)
            {
                _logger.LogInformation("No se encontró carrito para el usuario {UsuarioId}", usuarioId);
                return new CarritoDTO { Id = 0, UsuarioId = usuarioId };
            }
            return _mapper.Map<CarritoDTO>(carrito);
        }

        public async Task<(IEnumerable<CarritoDTO> Carritos, int Total)> GetCarritosAsync(int page, int pageSize, string? filtro)
        {
            _logger.LogInformation("Obteniendo carritos paginados. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            var carritos = await _repository.GetAllAsync();

            var total = carritos.Count();

            var carritosPaginados = carritos
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var carritosDto = _mapper.Map<IEnumerable<CarritoDTO>>(carritosPaginados);

            return (carritosDto, total);
        }

        public async Task<CarritoDTO> UpdateAmountProductoAsync(int usuarioId, int productoId, int cantidad)
        {
            _logger.LogInformation("Actualizando cantidad del producto {ProductoId} en el carrito del usuario {UsuarioId} a {Cantidad}", productoId, usuarioId, cantidad);

            var carrito = await _repository.GetByUsuarioIdAsync(usuarioId);
            if (carrito == null)
            {
                _logger.LogWarning("Carrito no encontrado para el usuario {UsuarioId}", usuarioId);
                throw new InvalidOperationException("El carrito del usuario especificado no existe.");
            }

            var detalle = carrito.CarritosDetalles?.FirstOrDefault(cd => cd.ProductoId == productoId);
            if (detalle == null)
            {
                _logger.LogWarning("Producto {ProductoId} no está en el carrito del usuario {UsuarioId}", productoId, usuarioId);
                throw new InvalidOperationException("El producto especificado no está en el carrito.");
            }

            if (cantidad <= 0)
            {
                _logger.LogWarning("Cantidad inválida: {Cantidad} para el usuario {UsuarioId}", cantidad, usuarioId);
                throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
            }

            var producto = await _productoRepository.GetByIdAsync(productoId);
            if (producto == null)
            {
                _logger.LogWarning("Producto no encontrado: {ProductoId}", productoId);
                throw new InvalidOperationException("El producto no existe.");
            }

            if (producto.Stock < cantidad)
            {
                _logger.LogWarning("Stock insuficiente para producto {ProductoId}. Stock disponible: {Stock}, Solicitado: {Cantidad}", productoId, producto.Stock, cantidad);
                throw new InvalidOperationException("No hay suficiente stock disponible.");
            }

            var carritoActualizado = await _repository.UpdateAsync(usuarioId, productoId, cantidad);
            if (carritoActualizado == null)
            {
                _logger.LogError("No se pudo actualizar la cantidad del producto {ProductoId} en el carrito del usuario {UsuarioId}", productoId, usuarioId);
                throw new InvalidOperationException("No se pudo actualizar la cantidad del producto en el carrito.");
            }

            _logger.LogInformation("Cantidad del producto {ProductoId} actualizada correctamente en el carrito del usuario {UsuarioId}", productoId, usuarioId);
            return _mapper.Map<CarritoDTO>(carritoActualizado);
        }
    }
}
