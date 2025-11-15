using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BicTechBack.UnitTests.Services
{
    public class PedidoServiceTests
    {
        [Fact]
        public async Task CreatePedidoAsync_PedidoValido_CreaPedidoYRetornaDTO()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var usuario = new Usuario { Id = 1, Nombre = "Usuario Test" };
            var producto = new Producto { Id = 1, Nombre = "Producto Test", Stock = 10, Precio = 100 };
            var dto = new CrearPedidoDTO
            {
                UsuarioId = 1,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = 1, Cantidad = 2, Precio = 100 }
                }
            };

            mockUsuarioRepo.Setup(r => r.GetByIdAsync(dto.UsuarioId)).ReturnsAsync(usuario);
            mockProductoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(producto);
            mockProductoRepo.Setup(r => r.UpdateAsync(It.IsAny<Producto>())).ReturnsAsync(producto);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Pedido>())).ReturnsAsync(new Pedido { Id = 1, UsuarioId = 1, Total = 200, PedidosDetalles = new List<PedidoDetalle>() });
            mockMapper.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO { Id = 1, UsuarioId = 1, Total = 200 });

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.CreatePedidoAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(1, result.UsuarioId);
            Assert.Equal(200, result.Total);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Pedido>()), Times.Once);
        }

        [Fact]
        public async Task CreatePedidoAsync_UsuarioNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var dto = new CrearPedidoDTO
            {
                UsuarioId = 1,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = 1, Cantidad = 2, Precio = 100 }
                }
            };

            mockUsuarioRepo.Setup(r => r.GetByIdAsync(dto.UsuarioId)).ReturnsAsync((Usuario?)null);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreatePedidoAsync(dto));
        }

        [Fact]
        public async Task CreatePedidoAsync_SinDetalles_LanzaArgumentException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var usuario = new Usuario { Id = 1, Nombre = "Usuario Test" };
            var dto = new CrearPedidoDTO
            {
                UsuarioId = 1,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>()
            };

            mockUsuarioRepo.Setup(r => r.GetByIdAsync(dto.UsuarioId)).ReturnsAsync(usuario);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePedidoAsync(dto));
        }

        [Fact]
        public async Task CreatePedidoAsync_ProductoNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var usuario = new Usuario { Id = 1, Nombre = "Usuario Test" };
            var dto = new CrearPedidoDTO
            {
                UsuarioId = 1,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = 1, Cantidad = 2, Precio = 100 }
                }
            };

            mockUsuarioRepo.Setup(r => r.GetByIdAsync(dto.UsuarioId)).ReturnsAsync(usuario);
            mockProductoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Producto?)null);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreatePedidoAsync(dto));
        }

        [Fact]
        public async Task CreatePedidoAsync_StockInsuficiente_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var usuario = new Usuario { Id = 1, Nombre = "Usuario Test" };
            var producto = new Producto { Id = 1, Nombre = "Producto Test", Stock = 1, Precio = 100 };
            var dto = new CrearPedidoDTO
            {
                UsuarioId = 1,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = 1, Cantidad = 2, Precio = 100 }
                }
            };

            mockUsuarioRepo.Setup(r => r.GetByIdAsync(dto.UsuarioId)).ReturnsAsync(usuario);
            mockProductoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(producto);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePedidoAsync(dto));
        }

        [Fact]
        public async Task AgregarProductoAlPedidoAsync_PedidoYProductoValidos_AgregaProducto()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var pedido = new Pedido
            {
                Id = 1,
                UsuarioId = 1,
                PedidosDetalles = new List<PedidoDetalle>()
            };
            var dto = new AgregarProductoPedidoDTO
            {
                PedidoId = 1,
                ProductoId = 2,
                Cantidad = 1,
                Precio = 50
            };

            mockRepo.Setup(r => r.GetByIdAsync(dto.PedidoId)).ReturnsAsync(pedido);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(pedido);
            mockProductoRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Producto { Id = 2, Nombre = "Producto Test 2", Stock = 10, Precio = 50 });
            mockMapper.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO { Id = 1 });

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.AgregarProductoAlPedidoAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Pedido>()), Times.Once);
        }

        [Fact]
        public async Task AgregarProductoAlPedidoAsync_PedidoNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var dto = new AgregarProductoPedidoDTO
            {
                PedidoId = 1,
                ProductoId = 2,
                Cantidad = 1,
                Precio = 50
            };

            mockRepo.Setup(r => r.GetByIdAsync(dto.PedidoId)).ReturnsAsync((Pedido?)null);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.AgregarProductoAlPedidoAsync(dto));
        }

        [Fact]
        public async Task DeletePedidoAsync_EliminacionCorrecta_RetornaTrue()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.DeletePedidoAsync(1);

            Assert.True(result);
            mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeletePedidoAsync_PedidoNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeletePedidoAsync(1));
        }

        [Fact]
        public async Task GetAllPedidosAsync_PedidosExistentes_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var pedidos = new List<Pedido>
            {
                new Pedido { Id = 1, UsuarioId = 1 },
                new Pedido { Id = 2, UsuarioId = 2 }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(pedidos);
            mockMapper.Setup(m => m.Map<IEnumerable<PedidoDTO>>(It.IsAny<IEnumerable<Pedido>>()))
                .Returns(new List<PedidoDTO>
                {
                    new PedidoDTO { Id = 1, UsuarioId = 1 },
                    new PedidoDTO { Id = 2, UsuarioId = 2 }
                });

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetAllPedidosAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllPedidosAsync_NoPedidos_RetornaListaVacia()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Pedido>());
            mockMapper.Setup(m => m.Map<IEnumerable<PedidoDTO>>(It.IsAny<IEnumerable<Pedido>>()))
                .Returns(new List<PedidoDTO>());

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetAllPedidosAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPedidoByIdAsync_PedidoExistente_RetornaDTO()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var pedido = new Pedido { Id = 1, UsuarioId = 1 };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pedido);
            mockMapper.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO { Id = 1, UsuarioId = 1 });

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetPedidoByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetPedidoByIdAsync_PedidoNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Pedido?)null);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetPedidoByIdAsync(1));
        }

        [Fact]
        public async Task UpdatePedidoAsync_PedidoActualizado_RetornaDTO()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var pedidoExistente = new Pedido
            {
                Id = 1,
                UsuarioId = 1,
                DireccionEnvio = "Antigua",
                PedidosDetalles = new List<PedidoDetalle>
        {
            new PedidoDetalle { ProductoId = 1, Cantidad = 1, Precio = 100, Subtotal = 100 }
        }
            };
            var usuario = new Usuario { Id = 2, Nombre = "Usuario Nuevo" };
            var dto = new CrearPedidoDTO
            {
                UsuarioId = 2,
                DireccionEnvio = "Nueva Direccion",
                Detalles = new List<CrearPedidoDetalleDTO>
        {
            new CrearPedidoDetalleDTO { ProductoId = 2, Cantidad = 2, Precio = 50 }
        }
            };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pedidoExistente);
            mockUsuarioRepo.Setup(r => r.GetByIdAsync(dto.UsuarioId)).ReturnsAsync(usuario);
            mockProductoRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Producto { Id = 2, Nombre = "Producto Test 2", Stock = 10, Precio = 50 });
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(pedidoExistente);
            mockMapper.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO { Id = 1, UsuarioId = 2 });

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.UpdatePedidoAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(2, result.UsuarioId);
        }

        [Fact]
        public async Task UpdatePedidoAsync_PedidoNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var dto = new CrearPedidoDTO
            {
                UsuarioId = 1,
                DireccionEnvio = "Nueva Direccion",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = 2, Cantidad = 2, Precio = 50 }
                }
            };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Pedido?)null);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdatePedidoAsync(1, dto));
        }

        [Fact]
        public async Task UpdatePedidoAsync_UsuarioNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var pedidoExistente = new Pedido
            {
                Id = 1,
                UsuarioId = 1,
                DireccionEnvio = "Antigua",
                PedidosDetalles = new List<PedidoDetalle>()
            };
            var dto = new CrearPedidoDTO
            {
                UsuarioId = 2,
                DireccionEnvio = "Nueva Direccion",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = 2, Cantidad = 2, Precio = 50 }
                }
            };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pedidoExistente);
            mockUsuarioRepo.Setup(r => r.GetByIdAsync(dto.UsuarioId)).ReturnsAsync((Usuario?)null);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdatePedidoAsync(1, dto));
        }

        [Fact]
        public async Task UpdatePedidoAsync_SinDetalles_LanzaArgumentException()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var pedidoExistente = new Pedido
            {
                Id = 1,
                UsuarioId = 1,
                DireccionEnvio = "Antigua",
                PedidosDetalles = new List<PedidoDetalle>()
            };
            var usuario = new Usuario { Id = 1, Nombre = "Usuario Test" };
            var dto = new CrearPedidoDTO
            {
                UsuarioId = 1,
                DireccionEnvio = "Nueva Direccion",
                Detalles = new List<CrearPedidoDetalleDTO>()
            };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pedidoExistente);
            mockUsuarioRepo.Setup(r => r.GetByIdAsync(dto.UsuarioId)).ReturnsAsync(usuario);

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdatePedidoAsync(1, dto));
        }

        [Fact]
        public async Task GetPedidosByUsuarioIdAsync_PedidosExistentes_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var pedidos = new List<Pedido>
            {
                new Pedido { Id = 1, UsuarioId = 1 },
                new Pedido { Id = 2, UsuarioId = 1 }
            };

            mockRepo.Setup(r => r.GetByClienteIdAsync(1)).ReturnsAsync(pedidos);
            mockMapper.Setup(m => m.Map<IEnumerable<PedidoDTO>>(It.IsAny<IEnumerable<Pedido>>()))
                .Returns(new List<PedidoDTO>
                {
                    new PedidoDTO { Id = 1, UsuarioId = 1 },
                    new PedidoDTO { Id = 2, UsuarioId = 1 }
                });

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetPedidosByUsuarioIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetPedidosByUsuarioIdAsync_NoPedidos_RetornaListaVacia()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            mockRepo.Setup(r => r.GetByClienteIdAsync(1)).ReturnsAsync(new List<Pedido>());
            mockMapper.Setup(m => m.Map<IEnumerable<PedidoDTO>>(It.IsAny<IEnumerable<Pedido>>()))
                .Returns(new List<PedidoDTO>());

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetPedidosByUsuarioIdAsync(1);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPedidosAsync_PedidosExistentesYPaginacion_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IPedidoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PedidoService>>();

            var pedidos = new List<Pedido>
            {
                new Pedido { Id = 1, UsuarioId = 1 },
                new Pedido { Id = 2, UsuarioId = 2 },
                new Pedido { Id = 3, UsuarioId = 3 }
            };

            var page = 1;
            var pageSize = 2;
            var total = pedidos.Count();

            var pedidosPaginados = pedidos.Skip((page - 1) * pageSize).Take(pageSize);

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(pedidos);
            mockMapper.Setup(m => m.Map<IEnumerable<PedidoDTO>>(It.IsAny<IEnumerable<Pedido>>()))
                .Returns(pedidosPaginados.Select(p => new PedidoDTO { Id = p.Id, UsuarioId = p.UsuarioId }));

            var service = new PedidoService(
                mockRepo.Object,
                mockUsuarioRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetPedidosAsync(page, pageSize, null);

            Assert.NotNull(result);
            Assert.Equal(2, result.Pedidos.Count());
            Assert.Equal(total, result.Total);
        }
    }
}