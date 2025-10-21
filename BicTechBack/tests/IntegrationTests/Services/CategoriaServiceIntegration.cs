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
    public class CategoriaServiceIntegration
    {
        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoriaProfile>();
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

        private CategoriaService GetService(AppDbContext context)
        {
            var repo = new CategoriaRepository(context);
            var mapper = GetMapper();
            var logger = new LoggerFactory().CreateLogger<CategoriaService>();
            return new CategoriaService(repo, mapper, logger);
        }

        [Fact]
        public async Task CreateCategoriaAsync_Valido_CreaCategoria()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var dto = new CrearCategoriaDTO { Nombre = "Electrónica" };
            var result = await service.CreateCategoriaAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Electrónica", result.Nombre);
            Assert.True(result.Id > 0);

            var enDb = await context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Electrónica");
            Assert.NotNull(enDb);
        }

        [Fact]
        public async Task CreateCategoriaAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            context.Categorias.Add(new Categoria { Nombre = "Electrónica" });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearCategoriaDTO { Nombre = "Electrónica" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCategoriaAsync(dto));
        }

        [Fact]
        public async Task DeleteCategoriaAsync_Exito_EliminaCategoria()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.DeleteCategoriaAsync(categoria.Id);

            Assert.True(result);
            Assert.False(context.Categorias.Any(c => c.Id == categoria.Id));
        }

        [Fact]
        public async Task DeleteCategoriaAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteCategoriaAsync(999));
        }

        [Fact]
        public async Task GetAllCategoriasAsync_ExistenCategorias_RetornaLista()
        {
            using var context = GetDbContext();
            context.Categorias.AddRange(
                new Categoria { Nombre = "Electrónica" },
                new Categoria { Nombre = "Hogar" }
            );
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetAllCategoriasAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllCategoriasAsync_SinCategorias_RetornaVacio()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllCategoriasAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCategoriaByIdAsync_Exito()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetCategoriaByIdAsync(categoria.Id);

            Assert.NotNull(result);
            Assert.Equal(categoria.Id, result.Id);
            Assert.Equal("Electrónica", result.Nombre);
        }

        [Fact]
        public async Task GetCategoriaByIdAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCategoriaByIdAsync(999));
        }

        [Fact]
        public async Task GetCategoriasAsync_Paginado_RetornaCorrecto()
        {
            using var context = GetDbContext();
            context.Categorias.AddRange(
                new Categoria { Nombre = "Electrónica" },
                new Categoria { Nombre = "Hogar" }
            );
            await context.SaveChangesAsync();

            var service = GetService(context);

            var (result, total) = await service.GetCategoriasAsync(1, 1, null);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, total);
        }

        [Fact]
        public async Task UpdateCategoriaAsync_Exito_ActualizaCategoria()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearCategoriaDTO { Nombre = "Tecnología" };

            var result = await service.UpdateCategoriaAsync(categoria.Id, dto);

            Assert.NotNull(result);
            Assert.Equal("Tecnología", result.Nombre);

            var enDb = await context.Categorias.FindAsync(categoria.Id);
            Assert.Equal("Tecnología", enDb.Nombre);
        }

        [Fact]
        public async Task UpdateCategoriaAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);
            var dto = new CrearCategoriaDTO { Nombre = "Nueva" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateCategoriaAsync(999, dto));
        }

        [Fact]
        public async Task UpdateCategoriaAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var cat1 = new Categoria { Nombre = "Electrónica" };
            var cat2 = new Categoria { Nombre = "Hogar" };
            context.Categorias.AddRange(cat1, cat2);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearCategoriaDTO { Nombre = "Hogar" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateCategoriaAsync(cat1.Id, dto));
        }
    }
}