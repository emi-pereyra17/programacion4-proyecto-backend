using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Mappings;
using BicTechBack.src.Core.Services;
using BicTechBack.src.Infrastructure.Data;
using BicTechBack.src.Infrastructure.Logging;
using BicTechBack.src.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BicTechBack.IntegrationTests.Services
{
    public class PedidoServiceIntegration
    {
        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PedidoProfile>();
                cfg.AddProfile<PedidoDetalleProfile>(); 
                cfg.AddProfile<ProductoProfile>();
                cfg.AddProfile<UsuarioProfile>();
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

        private PedidoService GetService(AppDbContext context)
        {
            var repo = new PedidoRepository(context);
            var usuarioRepo = new UsuarioRepository(context);
            var productoRepo = new ProductoRepository(context);
            var mapper = GetMapper();
            var msLogger = new LoggerFactory().CreateLogger<PedidoService>();
            var logger = new LoggerAdapter<PedidoService>(msLogger);
            return new PedidoService(repo, usuarioRepo, productoRepo, mapper, logger);
        }

        private async Task<(Usuario usuario, Producto producto)> CrearUsuarioYProductoAsync(AppDbContext context, int stock = 10)
        {
            var pais = new Pais { Nombre = "España" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            var categoria = new Categoria { Nombre = "Electrónica" };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var usuario = new Usuario { Nombre = "Cliente", Email = "cliente@mail.com", Password = "1234", Rol = RolUsuario.User };
            var producto = new Producto
            {
                Nombre = "Producto1",
                Precio = 100,
                Stock = stock,
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Descripcion = "desc",
                ImagenUrl = ""
            };
            context.Usuarios.Add(usuario);
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            return (usuario, producto);
        }

        [Fact]
        public async Task CreatePedidoAsync_Valido_CreaPedido()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var dto = new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            };

            var result = await service.CreatePedidoAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(usuario.Id, result.UsuarioId);
            Assert.Equal(200, result.Total);
            Assert.Single(result.PedidosDetalles);
            Assert.Equal(producto.Id, result.PedidosDetalles[0].ProductoId);

            var productoEnDb = await context.Productos.FindAsync(producto.Id);
            Assert.Equal(8, productoEnDb.Stock); // Stock descontado
        }

        [Fact]
        public async Task CreatePedidoAsync_UsuarioNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "España" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            var categoria = new Categoria { Nombre = "Electrónica" };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var producto = new Producto
            {
                Nombre = "Producto1",
                Precio = 100,
                Stock = 10,
                CategoriaId = categoria.Id,
                MarcaId = marca.Id,
                Descripcion = "desc",
                ImagenUrl = ""
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var dto = new CrearPedidoDTO
            {
                UsuarioId = 999,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreatePedidoAsync(dto));
        }

        [Fact]
        public async Task CreatePedidoAsync_SinProductos_LanzaArgumentException()
        {
            using var context = GetDbContext();
            var (usuario, _) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var dto = new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>()
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePedidoAsync(dto));
        }

        [Fact]
        public async Task CreatePedidoAsync_ProductoNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var (usuario, _) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var dto = new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = 999, Cantidad = 2, Precio = 100 }
                }
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreatePedidoAsync(dto));
        }

        [Fact]
        public async Task CreatePedidoAsync_StockInsuficiente_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context, stock: 1);

            var service = GetService(context);

            var dto = new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePedidoAsync(dto));
        }

        [Fact]
        public async Task AgregarProductoAlPedidoAsync_Exito()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var pedido = await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            });

            var dto = new AgregarProductoPedidoDTO
            {
                PedidoId = pedido.Id,
                ProductoId = producto.Id,
                Cantidad = 1,
                Precio = 100
            };

            var result = await service.AgregarProductoAlPedidoAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(3, result.PedidosDetalles[0].Cantidad);
            Assert.Equal(300, result.Total);
        }

        [Fact]
        public async Task AgregarProductoAlPedidoAsync_PedidoNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var dto = new AgregarProductoPedidoDTO
            {
                PedidoId = 999,
                ProductoId = producto.Id,
                Cantidad = 1,
                Precio = 100
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.AgregarProductoAlPedidoAsync(dto));
        }

        [Fact]
        public async Task DeletePedidoAsync_Exito_EliminaPedido()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var pedido = await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            });

            var result = await service.DeletePedidoAsync(pedido.Id);

            Assert.True(result);
            Assert.False(context.Pedidos.Any(p => p.Id == pedido.Id));
        }

        [Fact]
        public async Task DeletePedidoAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeletePedidoAsync(999));
        }

        [Fact]
        public async Task GetAllPedidosAsync_ExistenPedidos_RetornaLista()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            });

            var result = await service.GetAllPedidosAsync();

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllPedidosAsync_SinPedidos_RetornaVacio()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllPedidosAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPedidoByIdAsync_Exito()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var pedido = await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            });

            var result = await service.GetPedidoByIdAsync(pedido.Id);

            Assert.NotNull(result);
            Assert.Equal(pedido.Id, result.Id);
        }

        [Fact]
        public async Task GetPedidoByIdAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetPedidoByIdAsync(999));
        }

        [Fact]
        public async Task UpdatePedidoAsync_Exito_ActualizaPedido()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var pedido = await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            });

            var dto = new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Nueva Direccion",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 1, Precio = 100 }
                }
            };

            var result = await service.UpdatePedidoAsync(pedido.Id, dto);

            Assert.NotNull(result);
            Assert.Equal("Nueva Direccion", result.DireccionEnvio);
            Assert.Single(result.PedidosDetalles);
            Assert.Equal(100, result.Total);
        }

        [Fact]
        public async Task UpdatePedidoAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var dto = new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Nueva Direccion",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 1, Precio = 100 }
                }
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdatePedidoAsync(999, dto));
        }

        [Fact]
        public async Task UpdatePedidoAsync_UsuarioNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var pedido = await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            });

            var dto = new CrearPedidoDTO
            {
                UsuarioId = 999,
                DireccionEnvio = "Nueva Direccion",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 1, Precio = 100 }
                }
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdatePedidoAsync(pedido.Id, dto));
        }

        [Fact]
        public async Task UpdatePedidoAsync_SinProductos_LanzaArgumentException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var pedido = await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            });

            var dto = new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Nueva Direccion",
                Detalles = new List<CrearPedidoDetalleDTO>()
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdatePedidoAsync(pedido.Id, dto));
        }

        [Fact]
        public async Task GetPedidosByUsuarioIdAsync_ExistenPedidos_RetornaLista()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            });

            var result = await service.GetPedidosByUsuarioIdAsync(usuario.Id);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetPedidosByUsuarioIdAsync_SinPedidos_RetornaVacio()
        {
            using var context = GetDbContext();
            var (usuario, _) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var result = await service.GetPedidosByUsuarioIdAsync(usuario.Id);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPedidosAsync_Paginado_RetornaCorrecto()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context, stock: 20);

            var service = GetService(context);

            await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Calle Falsa 123",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 2, Precio = 100 }
                }
            });

            await service.CreatePedidoAsync(new CrearPedidoDTO
            {
                UsuarioId = usuario.Id,
                DireccionEnvio = "Otra Direccion",
                Detalles = new List<CrearPedidoDetalleDTO>
                {
                    new CrearPedidoDetalleDTO { ProductoId = producto.Id, Cantidad = 1, Precio = 100 }
                }
            });

            var (result, total) = await service.GetPedidosAsync(1, 1, null);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, total);
        }
    }
}