using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Mappings;
using BicTechBack.src.Core.Services;
using BicTechBack.src.Infrastructure.Data;
using BicTechBack.src.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BicTechBack.IntegrationTests.Services
{
    public class ProductoServiceIntegration
    {
        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductoProfile>();
                cfg.AddProfile<CategoriaProfile>();
                cfg.AddProfile<MarcaProfile>();
            });
            return config.CreateMapper();
        }

        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private ProductoService GetService(AppDbContext context)
        {
            var repo = new ProductoRepository(context);
            var categoriaRepo = new CategoriaRepository(context);
            var marcaRepo = new MarcaRepository(context);
            var mapper = GetMapper();
            var logger = new LoggerFactory().CreateLogger<ProductoService>();
            return new ProductoService(repo, marcaRepo, categoriaRepo, mapper, logger);
        }

        private (Categoria, Marca) AddCategoriaYMarca(AppDbContext context)
        {
            var categoria = new Categoria { Nombre = "Electrónica" };
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Categorias.Add(categoria);
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            context.SaveChanges();
            return (categoria, marca);
        }

        [Fact]
        public async Task CreateProductoAsync_Valido_CreaProducto()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);

            var service = GetService(context);
            var dto = new CrearProductoDTO
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            };

            var result = await service.CreateProductoAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("TV", result.Nombre);
            Assert.Equal(5, result.Stock);

            var enDb = await context.Productos.FirstOrDefaultAsync(p => p.Nombre == "TV");
            Assert.NotNull(enDb);
        }

        [Fact]
        public async Task CreateProductoAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);
            context.Productos.Add(new Producto
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearProductoDTO
            {
                Nombre = "TV",
                Precio = 1200,
                Descripcion = "Otra TV",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 3,
                ImagenUrl = "http://img.com/tv2.jpg"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProductoAsync(dto));
        }

        [Fact]
        public async Task CreateProductoAsync_CategoriaNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);

            var service = GetService(context);
            var dto = new CrearProductoDTO
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = 999,
                MarcaId = marca.Id,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProductoAsync(dto));
        }

        [Fact]
        public async Task CreateProductoAsync_MarcaNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);

            var service = GetService(context);
            var dto = new CrearProductoDTO
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = categoria.Id,
                MarcaId = 999,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProductoAsync(dto));
        }

        [Fact]
        public async Task DeleteProductoAsync_Exito_EliminaProducto()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);
            var producto = new Producto
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.DeleteProductoAsync(producto.Id);

            Assert.True(result);
            Assert.False(context.Productos.Any(p => p.Id == producto.Id));
        }

        [Fact]
        public async Task DeleteProductoAsync_NoExiste_RetornaFalse()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var result = await service.DeleteProductoAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task GetAllProductosAsync_ExistenProductos_RetornaLista()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);
            context.Productos.AddRange(
                new Producto
                {
                    Nombre = "TV",
                    Precio = 1000,
                    Descripcion = "Smart TV",
                    CategoriaId = categoria.Id,
                    MarcaId = marca.Id,
                    Stock = 5,
                    ImagenUrl = "http://img.com/tv.jpg"
                },
                new Producto
                {
                    Nombre = "Radio",
                    Precio = 200,
                    Descripcion = "Radio FM",
                    CategoriaId = categoria.Id,
                    MarcaId = marca.Id,
                    Stock = 10,
                    ImagenUrl = "http://img.com/radio.jpg"
                }
            );
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetAllProductosAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllProductosAsync_SinProductos_RetornaVacio()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllProductosAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProductoByIdAsync_Exito()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);
            var producto = new Producto
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetProductoByIdAsync(producto.Id);

            Assert.NotNull(result);
            Assert.Equal(producto.Id, result.Id);
            Assert.Equal("TV", result.Nombre);
        }

        [Fact]
        public async Task GetProductoByIdAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetProductoByIdAsync(999));
        }

        [Fact]
        public async Task GetProductosAsync_Paginado_RetornaCorrecto()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);
            context.Productos.AddRange(
                new Producto
                {
                    Nombre = "TV",
                    Precio = 1000,
                    Descripcion = "Smart TV",
                    CategoriaId = categoria.Id,
                    MarcaId = marca.Id,
                    Stock = 5,
                    ImagenUrl = "http://img.com/tv.jpg"
                },
                new Producto
                {
                    Nombre = "Radio",
                    Precio = 200,
                    Descripcion = "Radio FM",
                    CategoriaId = categoria.Id,
                    MarcaId = marca.Id,
                    Stock = 10,
                    ImagenUrl = "http://img.com/radio.jpg"
                }
            );
            await context.SaveChangesAsync();

            var service = GetService(context);

            var (result, total) = await service.GetProductosAsync(1, 1, null);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, total);
        }

        [Fact]
        public async Task UpdateProductoAsync_Exito_ActualizaProducto()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);
            var producto = new Producto
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearProductoDTO
            {
                Nombre = "TV 4K",
                Precio = 1500,
                Descripcion = "Smart TV 4K",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 8,
                ImagenUrl = "http://img.com/tv4k.jpg"
            };

            var result = await service.UpdateProductoAsync(producto.Id, dto);

            Assert.NotNull(result);
            Assert.Equal("TV 4K", result.Nombre);
            Assert.Equal(8, result.Stock);

            var enDb = await context.Productos.FindAsync(producto.Id);
            Assert.Equal("TV 4K", enDb.Nombre);
            Assert.Equal(8, enDb.Stock);
        }

        [Fact]
        public async Task UpdateProductoAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);

            var service = GetService(context);
            var dto = new CrearProductoDTO
            {
                Nombre = "TV 4K",
                Precio = 1500,
                Descripcion = "Smart TV 4K",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 8,
                ImagenUrl = "http://img.com/tv4k.jpg"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateProductoAsync(999, dto));
        }

        [Fact]
        public async Task UpdateProductoAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);
            var producto1 = new Producto
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            };
            var producto2 = new Producto
            {
                Nombre = "Radio",
                Precio = 200,
                Descripcion = "Radio FM",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 10,
                ImagenUrl = "http://img.com/radio.jpg"
            };
            context.Productos.AddRange(producto1, producto2);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearProductoDTO
            {
                Nombre = "Radio",
                Precio = 1500,
                Descripcion = "Smart TV 4K",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 8,
                ImagenUrl = "http://img.com/tv4k.jpg"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateProductoAsync(producto1.Id, dto));
        }

        [Fact]
        public async Task UpdateProductoAsync_CategoriaNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);
            var producto = new Producto
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearProductoDTO
            {
                Nombre = "TV 4K",
                Precio = 1500,
                Descripcion = "Smart TV 4K",
                CategoriaId = 999,
                MarcaId = marca.Id,
                Stock = 8,
                ImagenUrl = "http://img.com/tv4k.jpg"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateProductoAsync(producto.Id, dto));
        }

        [Fact]
        public async Task UpdateProductoAsync_MarcaNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (categoria, marca) = AddCategoriaYMarca(context);
            var producto = new Producto
            {
                Nombre = "TV",
                Precio = 1000,
                Descripcion = "Smart TV",
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Stock = 5,
                ImagenUrl = "http://img.com/tv.jpg"
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearProductoDTO
            {
                Nombre = "TV 4K",
                Precio = 1500,
                Descripcion = "Smart TV 4K",
                CategoriaId = categoria.Id,
                MarcaId = 999,
                Stock = 8,
                ImagenUrl = "http://img.com/tv4k.jpg"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateProductoAsync(producto.Id, dto));
        }
    }
}