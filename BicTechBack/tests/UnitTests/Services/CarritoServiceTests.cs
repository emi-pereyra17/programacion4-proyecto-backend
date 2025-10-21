using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BicTechBack.UnitTests.Services
{
    public class CarritoServiceTests
    {
        [Fact]
        public async Task AddProductoToCarritoAsync_Valido_AgregaYRetornaDTO()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var cantidad = 3;
            var producto = new Producto { Id = productoId, Stock = 10 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle>() };

            mockProductoRepo.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync(producto);
            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);
            mockRepo.Setup(r => r.AddProductoAsync(usuarioId, productoId, cantidad)).ReturnsAsync(carrito);
            mockMapper.Setup(m => m.Map<CarritoDTO>(carrito)).Returns(new CarritoDTO { Id = 1, UsuarioId = usuarioId });

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            var result = await service.AddProductoToCarritoAsync(usuarioId, productoId, cantidad);

            Assert.NotNull(result);
            Assert.Equal(usuarioId, result.UsuarioId);
            mockRepo.Verify(r => r.AddProductoAsync(usuarioId, productoId, cantidad), Times.Once);
        }

        [Fact]
        public async Task AddProductoToCarritoAsync_CantidadInvalida_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddProductoToCarritoAsync(1, 2, 0));
        }

        [Fact]
        public async Task AddProductoToCarritoAsync_ProductoNoExiste_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            mockProductoRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Producto?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddProductoToCarritoAsync(1, 2, 1));
        }

        [Fact]
        public async Task AddProductoToCarritoAsync_StockInsuficiente_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var cantidad = 5;
            var producto = new Producto { Id = productoId, Stock = 3 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle>() };

            mockProductoRepo.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync(producto);
            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddProductoToCarritoAsync(usuarioId, productoId, cantidad));
        }

        [Fact]
        public async Task AddProductoToCarritoAsync_ErrorEnRepositorio_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var cantidad = 1;
            var producto = new Producto { Id = productoId, Stock = 10 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle>() };

            mockProductoRepo.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync(producto);
            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);
            mockRepo.Setup(r => r.AddProductoAsync(usuarioId, productoId, cantidad)).ReturnsAsync((Carrito?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddProductoToCarritoAsync(usuarioId, productoId, cantidad));
        }

        [Fact]
        public async Task ClearCarritoAsync_UsuarioValido_LimpiaYRetornaDTO()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuario = new Usuario { Id = 1 };
            var carrito = new Carrito { Id = 1, UsuarioId = 1 };

            mockUsuarioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
            mockRepo.Setup(r => r.ClearAsync(1)).ReturnsAsync(carrito);
            mockMapper.Setup(m => m.Map<CarritoDTO>(carrito)).Returns(new CarritoDTO { Id = 1, UsuarioId = 1 });

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            var result = await service.ClearCarritoAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.UsuarioId);
            mockRepo.Verify(r => r.ClearAsync(1), Times.Once);
        }

        [Fact]
        public async Task ClearCarritoAsync_UsuarioNoExiste_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            mockUsuarioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Usuario?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ClearCarritoAsync(1));
        }

        [Fact]
        public async Task ClearCarritoAsync_ErrorEnRepositorio_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuario = new Usuario { Id = 1 };

            mockUsuarioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
            mockRepo.Setup(r => r.ClearAsync(1)).ReturnsAsync((Carrito?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ClearCarritoAsync(1));
        }

        [Fact]
        public async Task DeleteProductoFromCarritoAsync_ProductoEnCarrito_EliminaYRetornaDTO()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var detalle = new CarritoDetalle { ProductoId = productoId, Cantidad = 1 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle> { detalle } };

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);
            mockRepo.Setup(r => r.DeleteAsync(usuarioId, productoId)).ReturnsAsync(carrito);
            mockMapper.Setup(m => m.Map<CarritoDTO>(carrito)).Returns(new CarritoDTO { Id = 1, UsuarioId = usuarioId });

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            var result = await service.DeleteProductoFromCarritoAsync(usuarioId, productoId);

            Assert.NotNull(result);
            Assert.Equal(usuarioId, result.UsuarioId);
            mockRepo.Verify(r => r.DeleteAsync(usuarioId, productoId), Times.Once);
        }

        [Fact]
        public async Task DeleteProductoFromCarritoAsync_CarritoNoExiste_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(1)).ReturnsAsync((Carrito?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteProductoFromCarritoAsync(1, 2));
        }

        [Fact]
        public async Task DeleteProductoFromCarritoAsync_ProductoNoEnCarrito_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var carrito = new Carrito { Id = 1, UsuarioId = 1, CarritosDetalles = new List<CarritoDetalle>() };

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(1)).ReturnsAsync(carrito);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteProductoFromCarritoAsync(1, 2));
        }

        [Fact]
        public async Task DeleteProductoFromCarritoAsync_ErrorEnRepositorio_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var detalle = new CarritoDetalle { ProductoId = productoId, Cantidad = 1 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle> { detalle } };

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);
            mockRepo.Setup(r => r.DeleteAsync(usuarioId, productoId)).ReturnsAsync((Carrito?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteProductoFromCarritoAsync(usuarioId, productoId));
        }

        [Fact]
        public async Task GetAllCarritosAsync_ExistenCarritos_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var carritos = new List<Carrito>
            {
                new Carrito { Id = 1, UsuarioId = 1 },
                new Carrito { Id = 2, UsuarioId = 2 }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(carritos);
            mockMapper.Setup(m => m.Map<IEnumerable<CarritoDTO>>(It.IsAny<IEnumerable<Carrito>>()))
                .Returns(new List<CarritoDTO>
                {
                    new CarritoDTO { Id = 1, UsuarioId = 1 },
                    new CarritoDTO { Id = 2, UsuarioId = 2 }
                });

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            var result = await service.GetAllCarritosAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllCarritosAsync_NoCarritos_RetornaListaVacia()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Carrito>());
            mockMapper.Setup(m => m.Map<IEnumerable<CarritoDTO>>(It.IsAny<IEnumerable<Carrito>>()))
                .Returns(new List<CarritoDTO>());

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            var result = await service.GetAllCarritosAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCarritoByUsuarioIdAsync_ExisteCarrito_RetornaDTO()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var carrito = new Carrito { Id = 1, UsuarioId = 1 };
            mockRepo.Setup(r => r.GetByUsuarioIdAsync(1)).ReturnsAsync(carrito);
            mockMapper.Setup(m => m.Map<CarritoDTO>(It.IsAny<Carrito>())).Returns(new CarritoDTO { Id = 1, UsuarioId = 1 });

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            var result = await service.GetCarritoByUsuarioIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetCarritoByUsuarioIdAsync_NoExisteCarrito_RetornaDTOVacio()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(1)).ReturnsAsync((Carrito?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            var result = await service.GetCarritoByUsuarioIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal(1, result.UsuarioId);
        }

        [Fact]
        public async Task GetCarritosAsync_ExistenCarritosYPaginacion_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var carritos = new List<Carrito>
            {
                new Carrito { Id = 1, UsuarioId = 1 },
                new Carrito { Id = 2, UsuarioId = 2 },
                new Carrito { Id = 3, UsuarioId = 3 }
            };

            var page = 1;
            var pageSize = 2;
            var total = carritos.Count();

            var carritosPaginados = carritos.Skip((page - 1) * pageSize).Take(pageSize);

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(carritos);
            mockMapper.Setup(m => m.Map<IEnumerable<CarritoDTO>>(It.IsAny<IEnumerable<Carrito>>()))
                .Returns(carritosPaginados.Select(c => new CarritoDTO { Id = c.Id, UsuarioId = c.UsuarioId }));

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            var result = await service.GetCarritosAsync(page, pageSize, null);

            Assert.NotNull(result);
            Assert.Equal(2, result.Carritos.Count());
            Assert.Equal(total, result.Total);
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_ProductoEnCarrito_ActualizaYRetornaDTO()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var cantidad = 5;
            var detalle = new CarritoDetalle { ProductoId = productoId, Cantidad = 1 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle> { detalle } };
            var producto = new Producto { Id = productoId, Stock = 10 };

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);
            mockProductoRepo.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync(producto);
            mockRepo.Setup(r => r.UpdateAsync(usuarioId, productoId, cantidad)).ReturnsAsync(carrito);
            mockMapper.Setup(m => m.Map<CarritoDTO>(carrito)).Returns(new CarritoDTO { Id = 1, UsuarioId = usuarioId });

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            var result = await service.UpdateAmountProductoAsync(usuarioId, productoId, cantidad);

            Assert.NotNull(result);
            Assert.Equal(usuarioId, result.UsuarioId);
            mockRepo.Verify(r => r.UpdateAsync(usuarioId, productoId, cantidad), Times.Once);
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_CarritoNoExiste_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(1)).ReturnsAsync((Carrito?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(1, 2, 1));
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_ProductoNoEnCarrito_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var carrito = new Carrito { Id = 1, UsuarioId = 1, CarritosDetalles = new List<CarritoDetalle>() };

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(1)).ReturnsAsync(carrito);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(1, 2, 1));
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_CantidadInvalida_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var detalle = new CarritoDetalle { ProductoId = productoId, Cantidad = 1 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle> { detalle } };

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(usuarioId, productoId, 0));
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_ProductoNoExiste_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var detalle = new CarritoDetalle { ProductoId = productoId, Cantidad = 1 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle> { detalle } };

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);
            mockProductoRepo.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync((Producto?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(usuarioId, productoId, 1));
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_StockInsuficiente_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var cantidad = 5;
            var detalle = new CarritoDetalle { ProductoId = productoId, Cantidad = 1 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle> { detalle } };
            var producto = new Producto { Id = productoId, Stock = 2 };

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);
            mockProductoRepo.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync(producto);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(usuarioId, productoId, cantidad));
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_ErrorEnRepositorio_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICarritoRepository>();
            var mockProductoRepo = new Mock<IProductoRepository>();
            var mockUsuarioRepo = new Mock<IUsuarioRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CarritoService>>();

            var usuarioId = 1;
            var productoId = 2;
            var cantidad = 2;
            var detalle = new CarritoDetalle { ProductoId = productoId, Cantidad = 1 };
            var carrito = new Carrito { Id = 1, UsuarioId = usuarioId, CarritosDetalles = new List<CarritoDetalle> { detalle } };
            var producto = new Producto { Id = productoId, Stock = 10 };

            mockRepo.Setup(r => r.GetByUsuarioIdAsync(usuarioId)).ReturnsAsync(carrito);
            mockProductoRepo.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync(producto);
            mockRepo.Setup(r => r.UpdateAsync(usuarioId, productoId, cantidad)).ReturnsAsync((Carrito?)null);

            var service = new CarritoService(
                mockRepo.Object, mockMapper.Object, mockUsuarioRepo.Object, mockProductoRepo.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(usuarioId, productoId, cantidad));
        }
    }
}