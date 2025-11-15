using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicTechBack.UnitTests.Services
{
    public class ProductoServiceTests
    {
        [Fact]
        public async Task CreateProductoAsync_ProductoValido_CreaProductoYRetornaDTO()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();

            var dto = new CrearProductoDTO
            {
                Nombre = "Producto Test",
                Precio = 100,
                Descripcion = "Descripción Test",
                CategoriaId = 1,
                MarcaId = 1,
                Stock = 10,
                ImagenUrl = "http://example.com/imagen.jpg"
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producto>());
            mockCategoriaRepo.Setup(r => r.ExistsAsync(dto.CategoriaId)).ReturnsAsync(true);
            mockMarcaRepo.Setup(r => r.ExistsAsync(dto.MarcaId)).ReturnsAsync(true);

            var producto = new Producto { Id = 1, Nombre = dto.Nombre };
            mockMapper.Setup(m => m.Map<Producto>(dto)).Returns(producto);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Producto>())).ReturnsAsync(producto);
            mockMapper.Setup(m => m.Map<ProductoDTO>(producto)).Returns(new ProductoDTO { Id = 1, Nombre = dto.Nombre });

            var service = new ProductoService(
                mockRepo.Object,
                mockMarcaRepo.Object,
                mockCategoriaRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.CreateProductoAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Nombre, result.Nombre);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Producto>()), Times.Once);
        }

        [Fact]
        public async Task CreateProductoAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();

            var dto = new CrearProductoDTO
            {
                Nombre = "Producto Test",
                Precio = 100,
                Descripcion = "Descripción Test",
                CategoriaId = 1,
                MarcaId = 1,
                Stock = 10,
                ImagenUrl = "http://example.com/imagen.jpg"
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producto>
            {
                new Producto { Id = 1, Nombre = dto.Nombre }
            });

            var service = new ProductoService(
                mockRepo.Object,
                mockMarcaRepo.Object,
                mockCategoriaRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.CreateProductoAsync(dto);
            });
        }

        [Fact]
        public async Task CreateProductoAsync_CategoriaNoExiste_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();

            var dto = new CrearProductoDTO
            {
                Nombre = "Producto Test",
                Precio = 100,
                Descripcion = "Descripción Test",
                CategoriaId = 1,
                MarcaId = 1,
                Stock = 10,
                ImagenUrl = "http://example.com/imagen.jpg"
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producto>());
            mockCategoriaRepo.Setup(r => r.ExistsAsync(dto.CategoriaId)).ReturnsAsync(false);

            var service = new ProductoService(
                mockRepo.Object,
                mockMarcaRepo.Object,
                mockCategoriaRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.CreateProductoAsync(dto);
            });
        }

        [Fact]
        public async Task CreateProductoAsync_MarcaNoExiste_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();

            var dto = new CrearProductoDTO
            {
                Nombre = "Producto Test",
                Precio = 100,
                Descripcion = "Descripción Test",
                CategoriaId = 1,
                MarcaId = 99,
                Stock = 10,
                ImagenUrl = "http://example.com/imagen.jpg"
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producto>());
            mockCategoriaRepo.Setup(r => r.ExistsAsync(dto.CategoriaId)).ReturnsAsync(true);
            mockMarcaRepo.Setup(r => r.ExistsAsync(dto.MarcaId)).ReturnsAsync(false);

            var service = new ProductoService(
                mockRepo.Object,
                mockMarcaRepo.Object,
                mockCategoriaRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProductoAsync(dto));
        }

        [Fact]
        public async Task DeleteProductoAsync_EliminacionCorrecta_RetornaTrue()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();

            int productoId = 1;
            mockRepo.Setup(r => r.DeleteAsync(productoId)).ReturnsAsync(true);

            var service = new ProductoService(
                mockRepo.Object,
                null,
                null,
                null,
                mockLogger.Object
            );

            var result = await service.DeleteProductoAsync(productoId);
            Assert.True(result);

            mockRepo.Verify(r => r.DeleteAsync(productoId), Times.Once);
        }

        [Fact]
        public async Task DeleteProductoAsync_EliminacionFallida_RetornaFalse()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();

            int productoId = 2;

            mockRepo.Setup(r => r.DeleteAsync(productoId)).ReturnsAsync(false);
            var service = new ProductoService(
                mockRepo.Object,
                null,
                null,
                null,
                mockLogger.Object
            );

            var result = await service.DeleteProductoAsync(productoId);
            Assert.False(result);

            mockRepo.Verify(r => r.DeleteAsync(productoId), Times.Once);
        }

        [Fact]
        public async Task GetAllProductosAsync_ProductosExistentes_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();
            var mockMapper = new Mock<IMapper>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producto>
            {
                new Producto { Id = 1, Nombre = "Producto 1" },
                new Producto { Id = 2, Nombre = "Producto 2" }
            });

            mockMapper.Setup(m => m.Map<IEnumerable<ProductoDTO>>(It.IsAny<IEnumerable<Producto>>()))
                .Returns(new List<ProductoDTO>
                {
                    new ProductoDTO { Id = 1, Nombre = "Producto 1" },
                    new ProductoDTO { Id = 2, Nombre = "Producto 2" }
                });

            var service = new ProductoService(
                mockRepo.Object,
                null,
                null,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetAllProductosAsync();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Nombre == "Producto 1");
            Assert.Contains(result, p => p.Nombre == "Producto 2");
            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllProductosAsync_NoProductos_RetornaListaVacia()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();
            var mockMapper = new Mock<IMapper>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producto>());
            mockMapper.Setup(m => m.Map<IEnumerable<ProductoDTO>>(It.IsAny<IEnumerable<Producto>>()))
                .Returns(new List<ProductoDTO>());

            var service = new ProductoService(
                mockRepo.Object,
                null,
                null,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetAllProductosAsync();
            Assert.NotNull(result);
            Assert.Empty(result);
            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductoByIdAsync_ProductoExistente_RetornaDTO()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();
            var mockMapper = new Mock<IMapper>();

            int productoId = 1;
            mockRepo.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync(new Producto
            {
                Id = productoId,
                Nombre = "Producto Test"
            });

            mockMapper.Setup(m => m.Map<ProductoDTO>(It.IsAny<Producto>()))
                .Returns(new ProductoDTO { Id = productoId, Nombre = "Producto Test" });

            var service = new ProductoService(
                mockRepo.Object,
                null,
                null,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetProductoByIdAsync(productoId);
            Assert.NotNull(result);
            Assert.Equal(productoId, result.Id);
            Assert.Equal("Producto Test", result.Nombre);
            mockRepo.Verify(r => r.GetByIdAsync(productoId), Times.Once);
        }

        [Fact]
        public async Task GetProductoByIdAsync_ProductoNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();
            var mockMapper = new Mock<IMapper>();

            int productoId = 99;

            mockRepo.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync((Producto?)null);

            var service = new ProductoService(
                mockRepo.Object,
                null,
                null,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await service.GetProductoByIdAsync(productoId);
            });
            mockRepo.Verify(r => r.GetByIdAsync(productoId), Times.Once);
        }

        [Fact]
        public async Task GetProductosAsync_ProductosExistentesYPaginacion_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();
            var mockMapper = new Mock<IMapper>();

            var productos = new List<Producto>
            {
                new Producto { Id = 1, Nombre = "Producto 1" },
                new Producto { Id = 2, Nombre = "Producto 2" },
                new Producto { Id = 3, Nombre = "Producto 3" }
            };

            var page = 1;
            var pageSize = 2;
            var total = productos.Count();

            var productosPaginados = productos
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(productos);
            mockMapper.Setup(m => m.Map<IEnumerable<ProductoDTO>>(It.IsAny<IEnumerable<Producto>>()))
                .Returns(productosPaginados.Select(p => new ProductoDTO { Id = p.Id, Nombre = p.Nombre }));

            var service = new ProductoService(
                mockRepo.Object,
                null,
                null,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.GetProductosAsync(page, pageSize, null);
            Assert.NotNull(result);
            Assert.Equal(2, result.Productos.Count());
            Assert.Equal(total, result.Total);
            Assert.Contains(result.Productos, p => p.Nombre == "Producto 1");
            Assert.Contains(result.Productos, p => p.Nombre == "Producto 2");
            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProductoAsync_ProductoActualizado_RetornaDTO()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();

            var productoExistenteId = 1;
            var producto = new CrearProductoDTO
            {
                Nombre = "Producto Actualizado",
                Precio = 150,
                Descripcion = "Descripción Actualizada",
                CategoriaId = 1,
                MarcaId = 1,
                Stock = 20,
                ImagenUrl = "http://example.com/imagen_actualizada.jpg"
            };

            mockRepo.Setup(r => r.GetByIdAsync(productoExistenteId)).ReturnsAsync(new Producto
            {
                Id = productoExistenteId,
                Nombre = "Producto Antiguo"
            });
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producto>
            {
                new Producto { Id = 1, Nombre = "Producto Antiguo" },
                new Producto { Id = 2, Nombre = "Producto Antiguo 2" },
                new Producto { Id = 3, Nombre = "Producto Antiguo 3" }
            });

            mockCategoriaRepo.Setup(r => r.ExistsAsync(producto.CategoriaId)).ReturnsAsync(true);
            mockMarcaRepo.Setup(r => r.ExistsAsync(producto.MarcaId)).ReturnsAsync(true);
            mockMapper.Setup(m => m.Map<Producto>(producto)).Returns(new Producto
            {
                Id = productoExistenteId,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl
            });

            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Producto>())).ReturnsAsync(new Producto
            {
                Id = productoExistenteId,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl
            });

            mockMapper.Setup(m => m.Map<ProductoDTO>(It.IsAny<Producto>())).Returns(new ProductoDTO
            {
                Id = productoExistenteId,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
                Stock = producto.Stock
            });

            var service = new ProductoService(
                mockRepo.Object,
                mockMarcaRepo.Object,
                mockCategoriaRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            var result = await service.UpdateProductoAsync(productoExistenteId, producto);
            Assert.NotNull(result);
            Assert.Equal(producto.Nombre, result.Nombre);
            Assert.Equal(producto.Precio, result.Precio);
            Assert.Equal(producto.Descripcion, result.Descripcion);
            Assert.Equal(producto.Stock, result.Stock);
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Producto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductoAsync_ProductoNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();

            var productoExistenteId = 1;

            var producto = new CrearProductoDTO
            {
                Nombre = "Producto Actualizado",
                Precio = 150,
                Descripcion = "Descripción Actualizada",
                CategoriaId = 1,
                MarcaId = 1,
                Stock = 20,
                ImagenUrl = "http://example.com/imagen_actualizada.jpg"
            };

            mockRepo.Setup(r => r.GetByIdAsync(productoExistenteId)).ReturnsAsync((Producto?)null);
            mockCategoriaRepo.Setup(r => r.ExistsAsync(producto.CategoriaId)).ReturnsAsync(true);
            mockMarcaRepo.Setup(r => r.ExistsAsync(producto.MarcaId)).ReturnsAsync(true);
            mockMapper.Setup(m => m.Map<Producto>(producto)).Returns(new Producto
            {
                Id = productoExistenteId,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl
            });

            var service = new ProductoService(
                mockRepo.Object,
                mockMarcaRepo.Object,
                mockCategoriaRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await service.UpdateProductoAsync(productoExistenteId, producto);
            });

            mockRepo.Verify(r => r.GetByIdAsync(productoExistenteId), Times.Once);
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Producto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProductoAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();
            var productoExistenteId = 1;
            var producto = new CrearProductoDTO
            {
                Nombre = "Producto Duplicado",
                Precio = 150,
                Descripcion = "Descripción Actualizada",
                CategoriaId = 1,
                MarcaId = 1,
                Stock = 20,
                ImagenUrl = "http://example.com/imagen_actualizada.jpg"
            };
            mockRepo.Setup(r => r.GetByIdAsync(productoExistenteId)).ReturnsAsync(new Producto
            {
                Id = productoExistenteId,
                Nombre = "Producto Antiguo"
            });
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producto>
            {
                new Producto { Id = 1, Nombre = "Producto Antiguo" },
                new Producto { Id = 2, Nombre = "Producto Duplicado" }
            });
            mockCategoriaRepo.Setup(r => r.ExistsAsync(producto.CategoriaId)).ReturnsAsync(true);
            mockMarcaRepo.Setup(r => r.ExistsAsync(producto.MarcaId)).ReturnsAsync(true);
            mockMapper.Setup(m => m.Map<Producto>(producto)).Returns(new Producto
            {
                Id = productoExistenteId,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl
            });
            var service = new ProductoService(
                mockRepo.Object,
                mockMarcaRepo.Object,
                mockCategoriaRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.UpdateProductoAsync(productoExistenteId, producto);
            });
            mockRepo.Verify(r => r.GetByIdAsync(productoExistenteId), Times.Once);
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Producto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProductoAsync_CategoriaNoExiste_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();
            var productoExistenteId = 1;
            var producto = new CrearProductoDTO
            {
                Nombre = "Producto Actualizado",
                Precio = 150,
                Descripcion = "Descripción Actualizada",
                CategoriaId = 99, // Categoria no existente
                MarcaId = 1,
                Stock = 20,
                ImagenUrl = "http://example.com/imagen_actualizada.jpg"
            };
            mockRepo.Setup(r => r.GetByIdAsync(productoExistenteId)).ReturnsAsync(new Producto
            {
                Id = productoExistenteId,
                Nombre = "Producto Antiguo"
            });
            mockCategoriaRepo.Setup(r => r.ExistsAsync(producto.CategoriaId)).ReturnsAsync(false);
            mockMarcaRepo.Setup(r => r.ExistsAsync(producto.MarcaId)).ReturnsAsync(true);
            mockMapper.Setup(m => m.Map<Producto>(producto)).Returns(new Producto
            {
                Id = productoExistenteId,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl
            });
            var service = new ProductoService(
                mockRepo.Object,
                mockMarcaRepo.Object,
                mockCategoriaRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.UpdateProductoAsync(productoExistenteId, producto);
            });
            mockRepo.Verify(r => r.GetByIdAsync(productoExistenteId), Times.Once);
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Producto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProductoAsync_MarcaNoExiste_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IProductoRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<ProductoService>>();
            var productoExistenteId = 1;
            var producto = new CrearProductoDTO
            {
                Nombre = "Producto Actualizado",
                Precio = 150,
                Descripcion = "Descripción Actualizada",
                CategoriaId = 1,
                MarcaId = 99, // Marca no existente
                Stock = 20,
                ImagenUrl = "http://example.com/imagen_actualizada.jpg"
            };
            mockRepo.Setup(r => r.GetByIdAsync(productoExistenteId)).ReturnsAsync(new Producto
            {
                Id = productoExistenteId,
                Nombre = "Producto Antiguo"
            });
            mockCategoriaRepo.Setup(r => r.ExistsAsync(producto.CategoriaId)).ReturnsAsync(true);
            mockMarcaRepo.Setup(r => r.ExistsAsync(producto.MarcaId)).ReturnsAsync(false);
            mockMapper.Setup(m => m.Map<Producto>(producto)).Returns(new Producto
            {
                Id = productoExistenteId,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl
            });
            var service = new ProductoService(
                mockRepo.Object,
                mockMarcaRepo.Object,
                mockCategoriaRepo.Object,
                mockMapper.Object,
                mockLogger.Object
            );
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.UpdateProductoAsync(productoExistenteId, producto);
            });
            mockRepo.Verify(r => r.GetByIdAsync(productoExistenteId), Times.Once);
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Producto>()), Times.Never);
        }
    }
}