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

            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                _logger.LogWarning("Intento de crear pedido sin detalles. UsuarioId: {UsuarioId}", dto.UsuarioId);
                throw new ArgumentException("Debe agregar al menos un producto al pedido.");
            }

            decimal total = 0;

            var pedidoDetalles = new List<PedidoDetalle>();

            foreach (var detalleDto in dto.Detalles)
            {
                var producto = await _productoRepository.GetByIdAsync(detalleDto.ProductoId);
                if (producto == null)
                {
                    _logger.LogWarning("Producto no encontrado al crear pedido. ProductoId: {ProductoId}", detalleDto.ProductoId);
                    throw new KeyNotFoundException($"Producto con ID {detalleDto.ProductoId} no encontrado.");
                }

                if (producto.Stock < detalleDto.Cantidad)
                {
                    _logger.LogWarning("Stock insuficiente para producto {ProductoId}. Disponible: {Stock}, Solicitado: {Cantidad}",
                        detalleDto.ProductoId, producto.Stock, detalleDto.Cantidad);
                    throw new InvalidOperationException($"Stock insuficiente para el producto con ID {detalleDto.ProductoId}.");
                }

                producto.Stock -= detalleDto.Cantidad;
                await _productoRepository.UpdateAsync(producto);

                var subtotal = producto.Precio * detalleDto.Cantidad;
                total += subtotal;

                pedidoDetalles.Add(new PedidoDetalle
                {
                    ProductoId = detalleDto.ProductoId,
                    Cantidad = detalleDto.Cantidad,
                    Precio = producto.Precio,
                    Subtotal = subtotal
                });
            }

            var pedido = new Pedido
            {
                UsuarioId = dto.UsuarioId,
                DireccionEnvio = dto.DireccionEnvio,
                FechaPedido = DateTime.UtcNow,
                Estado = EstadoPedido.Pendiente,
                Total = total,
                PedidosDetalles = pedidoDetalles
            };

            var pedidoCreado = await _repository.AddAsync(pedido);

            _logger.LogInformation("Pedido creado correctamente. PedidoId: {PedidoId}, UsuarioId: {UsuarioId}",
                pedidoCreado.Id, pedidoCreado.UsuarioId);

            return _mapper.Map<PedidoDTO>(pedidoCreado);
        }

        public async Task<PedidoDTO> AgregarProductoAlPedidoAsync(AgregarProductoPedidoDTO dto)
        {
            _logger.LogInformation("Agregando producto {ProductoId} al pedido {PedidoId}", dto.ProductoId, dto.PedidoId);

            var pedido = await _repository.GetByIdAsync(dto.PedidoId);
            if (pedido == null)
                throw new KeyNotFoundException("Pedido no encontrado.");

            var producto = await _productoRepository.GetByIdAsync(dto.ProductoId);
            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {dto.ProductoId} no encontrado.");

            var detalleExistente = pedido.PedidosDetalles
                .FirstOrDefault(d => d.ProductoId == dto.ProductoId);

            if (detalleExistente != null)
            {
                detalleExistente.Cantidad += dto.Cantidad;
                detalleExistente.Subtotal = detalleExistente.Cantidad * detalleExistente.Precio;
                _logger.LogInformation("Cantidad actualizada para producto {ProductoId} en pedido {PedidoId}",
                    dto.ProductoId, dto.PedidoId);
            }
            else
            {
                var nuevoDetalle = new PedidoDetalle
                {
                    ProductoId = dto.ProductoId,
                    Cantidad = dto.Cantidad,
                    Precio = producto.Precio,
                    Subtotal = producto.Precio * dto.Cantidad
                };
                pedido.PedidosDetalles.Add(nuevoDetalle);
                _logger.LogInformation("Producto {ProductoId} agregado al pedido {PedidoId}",
                    dto.ProductoId, dto.PedidoId);
            }

            pedido.Total = pedido.PedidosDetalles.Sum(d => d.Subtotal);
            await _repository.UpdateAsync(pedido);

            return _mapper.Map<PedidoDTO>(pedido);
        }

        public async Task<bool> DeletePedidoAsync(int id)
        {
            _logger.LogInformation("Intentando eliminar pedido. PedidoId: {PedidoId}", id);

            var eliminado = await _repository.DeleteAsync(id);
            if (!eliminado)
                throw new KeyNotFoundException("Pedido no encontrado.");

            _logger.LogInformation("Pedido eliminado correctamente. PedidoId: {PedidoId}", id);
            return true;
        }

        public async Task<IEnumerable<PedidoDTO>> GetAllPedidosAsync()
        {
            _logger.LogInformation("Obteniendo todos los pedidos.");
            var pedidos = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PedidoDTO>>(pedidos);
        }

        public async Task<PedidoDTO> GetPedidoByIdAsync(int id)
        {
            var pedido = await _repository.GetByIdAsync(id);
            if (pedido == null)
                throw new KeyNotFoundException("Pedido no encontrado.");
            return _mapper.Map<PedidoDTO>(pedido);
        }

        public async Task<IEnumerable<PedidoDTO>> GetPedidosByUsuarioIdAsync(int usuarioId)
        {
            _logger.LogInformation("Obteniendo pedidos para usuario. UsuarioId: {UsuarioId}", usuarioId);
            var pedidos = await _repository.GetByClienteIdAsync(usuarioId);
            return _mapper.Map<IEnumerable<PedidoDTO>>(pedidos);
        }

        public async Task<(IEnumerable<PedidoDTO> Pedidos, int Total)> GetPedidosAsync(int page, int pageSize, string? filtro)
        {
            _logger.LogInformation("Obteniendo pedidos paginados. Página: {Page}, Tamaño: {PageSize}, Filtro: {Filtro}",
                page, pageSize, filtro);

            var pedidos = await _repository.GetAllAsync();
            var total = pedidos.Count();

            var pedidosPaginados = pedidos
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var pedidosDto = _mapper.Map<IEnumerable<PedidoDTO>>(pedidosPaginados);

            return (pedidosDto, total);
        }

        public async Task<PedidoDTO> UpdatePedidoAsync(int id, CrearPedidoDTO dto)
        {
            _logger.LogInformation("Intentando actualizar pedido. PedidoId: {PedidoId}", id);

            var pedidoExistente = await _repository.GetByIdAsync(id);
            if (pedidoExistente == null)
                throw new KeyNotFoundException("Pedido no encontrado.");

            var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuario no encontrado.");

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ArgumentException("Debe agregar al menos un producto al pedido.");

            // Recalcular todo el pedido
            pedidoExistente.UsuarioId = dto.UsuarioId;
            pedidoExistente.DireccionEnvio = dto.DireccionEnvio;
            pedidoExistente.Estado = (EstadoPedido)dto.Estado;
            pedidoExistente.PedidosDetalles.Clear();

            decimal total = 0;

            foreach (var detalleDto in dto.Detalles)
            {
                var producto = await _productoRepository.GetByIdAsync(detalleDto.ProductoId);
                if (producto == null)
                    throw new KeyNotFoundException($"Producto con ID {detalleDto.ProductoId} no encontrado.");

                var subtotal = producto.Precio * detalleDto.Cantidad;
                total += subtotal;

                pedidoExistente.PedidosDetalles.Add(new PedidoDetalle
                {
                    ProductoId = detalleDto.ProductoId,
                    Cantidad = detalleDto.Cantidad,
                    Precio = producto.Precio,
                    Subtotal = subtotal
                });
            }

            pedidoExistente.Total = total;

            var pedidoActualizado = await _repository.UpdateAsync(pedidoExistente);

            _logger.LogInformation("Pedido actualizado correctamente. PedidoId: {PedidoId}", pedidoActualizado.Id);
            return _mapper.Map<PedidoDTO>(pedidoActualizado);
        }
    }
}
