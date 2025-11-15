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
    public class CarritoServiceIntegration
    {
        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarritoProfile>();
                cfg.AddProfile<CarritoDetalleProfile>();
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

        private CarritoService GetService(AppDbContext context)
        {
            var repo = new CarritoRepository(context);
            var productoRepo = new ProductoRepository(context);
            var usuarioRepo = new UsuarioRepository(context);
            var mapper = GetMapper();
            var msLogger = new LoggerFactory().CreateLogger<CarritoService>();
            var logger = new LoggerAdapter<CarritoService>(msLogger);
            return new CarritoService(repo, mapper, usuarioRepo, productoRepo, logger);
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
        public async Task AddProductoToCarritoAsync_Exito_AgregaProducto()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            var result = await service.AddProductoToCarritoAsync(usuario.Id, producto.Id, 2);

            Assert.NotNull(result);
            Assert.Equal(usuario.Id, result.UsuarioId);
            Assert.Single(result.CarritosDetalles);
            Assert.Equal(producto.Id, result.CarritosDetalles[0].ProductoId);
            Assert.Equal(2, result.CarritosDetalles[0].Cantidad);
        }

        [Fact]
        public async Task AddProductoToCarritoAsync_ProductoNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "España" };
            context.Paises.Add(pais);
            var usuario = new Usuario { Nombre = "Cliente", Email = "cliente@mail.com", Password = "1234", Rol = RolUsuario.User };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddProductoToCarritoAsync(usuario.Id, 999, 1));
        }

        [Fact]
        public async Task AddProductoToCarritoAsync_CantidadInvalida_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddProductoToCarritoAsync(usuario.Id, producto.Id, 0));
        }

        [Fact]
        public async Task AddProductoToCarritoAsync_StockInsuficiente_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context, stock: 1);

            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddProductoToCarritoAsync(usuario.Id, producto.Id, 2));
        }

        [Fact]
        public async Task DeleteProductoFromCarritoAsync_Exito_EliminaProducto()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);
            await service.AddProductoToCarritoAsync(usuario.Id, producto.Id, 2);

            var result = await service.DeleteProductoFromCarritoAsync(usuario.Id, producto.Id);

            Assert.NotNull(result);
            Assert.Empty(result.CarritosDetalles);
        }

        [Fact]
        public async Task DeleteProductoFromCarritoAsync_ProductoNoEnCarrito_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteProductoFromCarritoAsync(usuario.Id, producto.Id));
        }

        [Fact]
        public async Task DeleteProductoFromCarritoAsync_CarritoNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteProductoFromCarritoAsync(999, 1));
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_Exito_ActualizaCantidad()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);
            await service.AddProductoToCarritoAsync(usuario.Id, producto.Id, 2);

            var result = await service.UpdateAmountProductoAsync(usuario.Id, producto.Id, 5);

            Assert.NotNull(result);
            Assert.Single(result.CarritosDetalles);
            Assert.Equal(5, result.CarritosDetalles[0].Cantidad);
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_ProductoNoEnCarrito_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(usuario.Id, producto.Id, 2));
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_CarritoNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(999, 1, 2));
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_CantidadInvalida_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);
            await service.AddProductoToCarritoAsync(usuario.Id, producto.Id, 2);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(usuario.Id, producto.Id, 0));
        }

        [Fact]
        public async Task UpdateAmountProductoAsync_StockInsuficiente_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context, stock: 2);

            var service = GetService(context);
            await service.AddProductoToCarritoAsync(usuario.Id, producto.Id, 2);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAmountProductoAsync(usuario.Id, producto.Id, 5));
        }

        [Fact]
        public async Task ClearCarritoAsync_Exito_LimpiaCarrito()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);
            await service.AddProductoToCarritoAsync(usuario.Id, producto.Id, 2);

            var result = await service.ClearCarritoAsync(usuario.Id);

            Assert.NotNull(result);
            Assert.Empty(result.CarritosDetalles);
        }

        [Fact]
        public async Task ClearCarritoAsync_UsuarioNoExiste_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ClearCarritoAsync(999));
        }

        [Fact]
        public async Task GetCarritoByUsuarioIdAsync_Exito()
        {
            using var context = GetDbContext();
            var (usuario, producto) = await CrearUsuarioYProductoAsync(context);

            var service = GetService(context);
            await service.AddProductoToCarritoAsync(usuario.Id, producto.Id, 2);

            var result = await service.GetCarritoByUsuarioIdAsync(usuario.Id);

            Assert.NotNull(result);
            Assert.Equal(usuario.Id, result.UsuarioId);
            Assert.Single(result.CarritosDetalles);
        }

        [Fact]
        public async Task GetCarritoByUsuarioIdAsync_SinCarrito_RetornaDTOVacio()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "España" };
            context.Paises.Add(pais);
            var usuario = new Usuario { Nombre = "Cliente", Email = "cliente@mail.com", Password = "1234", Rol = RolUsuario.User };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetCarritoByUsuarioIdAsync(usuario.Id);

            Assert.NotNull(result);
            Assert.Equal(usuario.Id, result.UsuarioId);
            Assert.True(result.Id == 0 || result.CarritosDetalles == null || result.CarritosDetalles.Count == 0);
        }

        [Fact]
        public async Task GetAllCarritosAsync_ExistenCarritos_RetornaLista()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "España" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            var categoria = new Categoria { Nombre = "Electrónica" };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var usuario1 = new Usuario { Nombre = "Cliente1", Email = "c1@mail.com", Password = "1234", Rol = RolUsuario.User };
            var usuario2 = new Usuario { Nombre = "Cliente2", Email = "c2@mail.com", Password = "1234", Rol = RolUsuario.User };
            var producto = new Producto { Nombre = "Producto1", Precio = 100, Stock = 10, CategoriaId = categoria.Id, MarcaId = marca.Id, Descripcion = "desc", ImagenUrl = "" };
            context.Usuarios.AddRange(usuario1, usuario2);
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.AddProductoToCarritoAsync(usuario1.Id, producto.Id, 2);
            await service.AddProductoToCarritoAsync(usuario2.Id, producto.Id, 1);

            var result = await service.GetAllCarritosAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllCarritosAsync_SinCarritos_RetornaVacio()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllCarritosAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCarritosAsync_Paginado_RetornaCorrecto()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "España" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            var categoria = new Categoria { Nombre = "Electrónica" };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var usuario1 = new Usuario { Nombre = "Cliente1", Email = "c1@mail.com", Password = "1234", Rol = RolUsuario.User };
            var usuario2 = new Usuario { Nombre = "Cliente2", Email = "c2@mail.com", Password = "1234", Rol = RolUsuario.User };
            var producto = new Producto { Nombre = "Producto1", Precio = 100, Stock = 10, CategoriaId = categoria.Id, MarcaId = marca.Id, Descripcion = "desc", ImagenUrl = "" };
            context.Usuarios.AddRange(usuario1, usuario2);
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.AddProductoToCarritoAsync(usuario1.Id, producto.Id, 2);
            await service.AddProductoToCarritoAsync(usuario2.Id, producto.Id, 1);

            var (result, total) = await service.GetCarritosAsync(1, 1, null);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, total);
        }
    }
}