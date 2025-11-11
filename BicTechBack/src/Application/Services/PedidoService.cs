using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;

namespace BicTechBack.src.Core.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _repository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;
        private readonly IAppLogger<PedidoService> _logger;

        public PedidoService(
            IPedidoRepository repository,
            IUsuarioRepository usuarioRepository,
            IProductoRepository productoRepository,
            IMapper mapper,
            IAppLogger<PedidoService> logger)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
            _productoRepository = productoRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PedidoDTO> CreatePedidoAsync(CrearPedidoDTO dto)
        {
            _logger.LogInformation("Intentando crear pedido para usuario {UsuarioId}", dto.UsuarioId);

            var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado al crear pedido. UsuarioId: {UsuarioId}", dto.UsuarioId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            if (dto.Productos == null || !dto.Productos.Any())
            {
                _logger.LogWarning("Intento de crear pedido sin productos. UsuarioId: {UsuarioId}", dto.UsuarioId);
                throw new ArgumentException("Debe agregar al menos un producto al pedido.");
            }

            foreach (var prod in dto.Productos)
            {
                var producto = await _productoRepository.GetByIdAsync(prod.ProductoId);
                if (producto == null)
                {
                    _logger.LogWarning("Producto no encontrado al crear pedido. ProductoId: {ProductoId}", prod.ProductoId);
                    throw new KeyNotFoundException($"Producto con ID {prod.ProductoId} no encontrado.");
                }

                if (producto.Stock < prod.Cantidad)
                {
                    _logger.LogWarning("Stock insuficiente para producto {ProductoId} al crear pedido. Stock disponible: {Stock}, Solicitado: {Cantidad}", prod.ProductoId, producto.Stock, prod.Cantidad);
                    throw new InvalidOperationException($"Stock insuficiente para el producto con ID {prod.ProductoId}.");
                }

                producto.Stock -= prod.Cantidad;
                await _productoRepository.UpdateAsync(producto);
            }

            decimal total = dto.Productos.Sum(p => p.Precio * p.Cantidad);

            var pedido = new Pedido
            {
                UsuarioId = dto.UsuarioId,
                DireccionEnvio = dto.DireccionEnvio,
                FechaPedido = DateTime.UtcNow,
                Estado = EstadoPedido.Pendiente,
                Total = total,
                PedidosDetalles = dto.Productos.Select(p => new PedidoDetalle
                {
                    ProductoId = p.ProductoId,
                    Cantidad = p.Cantidad,
                    Precio = p.Precio,
                    Subtotal = p.Precio * p.Cantidad
                }).ToList()
            };

            var pedidoCreado = await _repository.AddAsync(pedido);

            _logger.LogInformation("Pedido creado correctamente. PedidoId: {PedidoId}, UsuarioId: {UsuarioId}", pedidoCreado.Id, pedidoCreado.UsuarioId);

            return _mapper.Map<PedidoDTO>(pedidoCreado);
        }

        public async Task<PedidoDTO> AgregarProductoAlPedidoAsync(AgregarProductoPedidoDTO dto)
        {
            _logger.LogInformation("Agregando producto {ProductoId} al pedido {PedidoId}", dto.ProductoId, dto.PedidoId);

            var pedido = await _repository.GetByIdAsync(dto.PedidoId);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido no encontrado al agregar producto. PedidoId: {PedidoId}", dto.PedidoId);
                throw new KeyNotFoundException("Pedido no encontrado.");
            }

            var detalleExistente = pedido.PedidosDetalles
                .FirstOrDefault(d => d.ProductoId == dto.ProductoId);

            if (detalleExistente != null)
            {
                detalleExistente.Cantidad += dto.Cantidad;
                detalleExistente.Subtotal = detalleExistente.Cantidad * detalleExistente.Precio;
                _logger.LogInformation("Cantidad actualizada para producto {ProductoId} en pedido {PedidoId}", dto.ProductoId, dto.PedidoId);
            }
            else
            {
                var nuevoDetalle = new PedidoDetalle
                {
                    ProductoId = dto.ProductoId,
                    Cantidad = dto.Cantidad,
                    Precio = dto.Precio,
                    Subtotal = dto.Precio * dto.Cantidad
                };
                pedido.PedidosDetalles.Add(nuevoDetalle);
                _logger.LogInformation("Producto {ProductoId} agregado al pedido {PedidoId}", dto.ProductoId, dto.PedidoId);
            }

            pedido.Total = pedido.PedidosDetalles.Sum(d => d.Subtotal);

            await _repository.UpdateAsync(pedido);

            return _mapper.Map<PedidoDTO>(pedido);
        }

        public async Task<bool> DeletePedidoAsync(int id)
        {
            _logger.LogInformation("Intentando eliminar pedido. PedidoId: {PedidoId}", id);

            var pedido = await _repository.DeleteAsync(id);
            if (!pedido)
            {
                _logger.LogWarning("Pedido no encontrado al intentar eliminar. PedidoId: {PedidoId}", id);
                throw new KeyNotFoundException("Pedido no encontrado.");
            }

            _logger.LogInformation("Pedido eliminado correctamente. PedidoId: {PedidoId}", id);
            return true;
        }

        public async Task<IEnumerable<PedidoDTO>> GetAllPedidosAsync()
        {
            _logger.LogInformation("Obteniendo todos los pedidos.");
            var pedidos = await _repository.GetAllAsync();
            if (pedidos == null || !pedidos.Any())
            {
                _logger.LogInformation("No se encontraron pedidos en la base de datos.");
                return Enumerable.Empty<PedidoDTO>();
            }
            return _mapper.Map<IEnumerable<PedidoDTO>>(pedidos);
        }

        public async Task<PedidoDTO> GetPedidoByIdAsync(int id)
        {
            _logger.LogInformation("Buscando pedido por Id: {PedidoId}", id);
            var pedido = await _repository.GetByIdAsync(id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido no encontrado. PedidoId: {PedidoId}", id);
                throw new KeyNotFoundException("Pedido no encontrado.");
            }
            return _mapper.Map<PedidoDTO>(pedido);
        }

        public async Task<PedidoDTO> UpdatePedidoAsync(int id, CrearPedidoDTO dto)
        {
            _logger.LogInformation("Intentando actualizar pedido. PedidoId: {PedidoId}", id);

            var pedidoExistente = await _repository.GetByIdAsync(id);
            if (pedidoExistente == null)
            {
                _logger.LogWarning("Pedido no encontrado al intentar actualizar. PedidoId: {PedidoId}", id);
                throw new KeyNotFoundException("Pedido no encontrado.");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado al actualizar pedido. UsuarioId: {UsuarioId}", dto.UsuarioId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            if (dto.Productos == null || !dto.Productos.Any())
            {
                _logger.LogWarning("Intento de actualizar pedido sin productos. PedidoId: {PedidoId}", id);
                throw new ArgumentException("Debe agregar al menos un producto al pedido.");
            }

            pedidoExistente.UsuarioId = dto.UsuarioId;
            pedidoExistente.DireccionEnvio = dto.DireccionEnvio;
            pedidoExistente.PedidosDetalles.Clear();

            foreach (var prod in dto.Productos)
            {
                pedidoExistente.PedidosDetalles.Add(new PedidoDetalle
                {
                    ProductoId = prod.ProductoId,
                    Cantidad = prod.Cantidad,
                    Precio = prod.Precio,
                    Subtotal = prod.Precio * prod.Cantidad
                });
            }

            pedidoExistente.Total = pedidoExistente.PedidosDetalles.Sum(d => d.Subtotal);

            var pedidoActualizado = await _repository.UpdateAsync(pedidoExistente);

            _logger.LogInformation("Pedido actualizado correctamente. PedidoId: {PedidoId}", pedidoActualizado.Id);

            return _mapper.Map<PedidoDTO>(pedidoActualizado);
        }

        public async Task<IEnumerable<PedidoDTO>> GetPedidosByUsuarioIdAsync(int usuarioId)
        {
            _logger.LogInformation("Obteniendo pedidos para usuario. UsuarioId: {UsuarioId}", usuarioId);
            var pedidos = await _repository.GetByClienteIdAsync(usuarioId);
            if (pedidos == null || !pedidos.Any())
            {
                _logger.LogInformation("No se encontraron pedidos para el usuario. UsuarioId: {UsuarioId}", usuarioId);
                return Enumerable.Empty<PedidoDTO>();
            }
            return _mapper.Map<IEnumerable<PedidoDTO>>(pedidos);
        }

        public async Task<(IEnumerable<PedidoDTO> Pedidos, int Total)> GetPedidosAsync(int page, int pageSize, string? filtro)
        {
            _logger.LogInformation("Obteniendo pedidos paginados. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}", page, pageSize, filtro);
            var pedidos = await _repository.GetAllAsync();

            var total = pedidos.Count();

            var pedidosPaginados = pedidos
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var pedidosDto = _mapper.Map<IEnumerable<PedidoDTO>>(pedidosPaginados);

            return (pedidosDto, total);
        }
    }
}