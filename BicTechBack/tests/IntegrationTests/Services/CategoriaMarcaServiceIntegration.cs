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
    public class CategoriaMarcaServiceIntegration
    {
        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoriaMarcaProfile>();
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

        private CategoriaMarcaService GetService(AppDbContext context)
        {
            var repo = new CategoriaMarcaRepository(context);
            var categoriaRepo = new CategoriaRepository(context);
            var marcaRepo = new MarcaRepository(context);
            var mapper = GetMapper();
            var msLogger = new LoggerFactory().CreateLogger<CategoriaMarcaService>();
            var logger = new LoggerAdapter<CategoriaMarcaService>(msLogger);
            return new CategoriaMarcaService(repo, categoriaRepo, marcaRepo, mapper, logger);
        }

        [Fact]
        public async Task CreateCMAsync_Valido_CreaRelacion()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Categorias.Add(categoria);
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearCategoriaMarcaDTO { CategoriaId = categoria.Id, MarcaId = marca.Id };

            var result = await service.CreateCMAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(categoria.Id, result.CategoriaId);
            Assert.Equal(marca.Id, result.MarcaId);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task CreateCMAsync_CategoriaNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearCategoriaMarcaDTO { CategoriaId = 999, MarcaId = marca.Id };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateCMAsync(dto));
        }

        [Fact]
        public async Task CreateCMAsync_MarcaNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearCategoriaMarcaDTO { CategoriaId = categoria.Id, MarcaId = 999 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateCMAsync(dto));
        }

        [Fact]
        public async Task CreateCMAsync_RelacionDuplicada_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Categorias.Add(categoria);
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            context.CategoriasMarcas.Add(new CategoriaMarca { CategoriaId = categoria.Id, MarcaId = marca.Id });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearCategoriaMarcaDTO { CategoriaId = categoria.Id, MarcaId = marca.Id };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCMAsync(dto));
        }

        [Fact]
        public async Task DeleteCMAsync_EliminacionCorrecta()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Categorias.Add(categoria);
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var cm = new CategoriaMarca { CategoriaId = categoria.Id, MarcaId = marca.Id };
            context.CategoriasMarcas.Add(cm);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.DeleteCMAsync(cm.Id);

            Assert.True(result);
            Assert.False(context.CategoriasMarcas.Any(x => x.Id == cm.Id));
        }

        [Fact]
        public async Task DeleteCMAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteCMAsync(999));
        }

        [Fact]
        public async Task GetAllCMAsync_ExistenRelaciones_RetornaLista()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Categorias.Add(categoria);
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            context.CategoriasMarcas.Add(new CategoriaMarca { CategoriaId = categoria.Id, MarcaId = marca.Id });
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetAllCMAsync();

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllCMAsync_NoRelaciones_RetornaVacio()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllCMAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMarcasPorCategoriaAsync_Exito()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Categorias.Add(categoria);
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            context.CategoriasMarcas.Add(new CategoriaMarca { CategoriaId = categoria.Id, MarcaId = marca.Id, Marca = marca });
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetMarcasPorCategoriaAsync(categoria.Id);

            Assert.NotNull(result);
            var marcaDto = Assert.Single(result);
            Assert.Equal(marca.Id, marcaDto.Id);
            Assert.Equal("Sony", marcaDto.Nombre);
        }

        [Fact]
        public async Task GetMarcasPorCategoriaAsync_CategoriaNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetMarcasPorCategoriaAsync(999));
        }

        [Fact]
        public async Task GetCategoriasPorMarcaAsync_Exito()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Categorias.Add(categoria);
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            context.CategoriasMarcas.Add(new CategoriaMarca { CategoriaId = categoria.Id, MarcaId = marca.Id, Categoria = categoria });
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetCategoriasPorMarcaAsync(marca.Id);

            Assert.NotNull(result);
            var categoriaDto = Assert.Single(result);
            Assert.Equal(categoria.Id, categoriaDto.Id);
            Assert.Equal("Electrónica", categoriaDto.Nombre);
        }

        [Fact]
        public async Task GetCategoriasPorMarcaAsync_MarcaNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCategoriasPorMarcaAsync(999));
        }

        [Fact]
        public async Task GetCMByCategoriaIdAsync_Exito()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Categorias.Add(categoria);
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var cm = new CategoriaMarca { CategoriaId = categoria.Id, MarcaId = marca.Id };
            context.CategoriasMarcas.Add(cm);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetCMByCategoriaIdAsync(categoria.Id);

            Assert.NotNull(result);
            var cmDto = Assert.Single(result);
            Assert.Equal(categoria.Id, cmDto.CategoriaId);
            Assert.Equal(marca.Id, cmDto.MarcaId);
        }

        [Fact]
        public async Task GetCMByCategoriaIdAsync_CategoriaNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCMByCategoriaIdAsync(999));
        }

        [Fact]
        public async Task GetCMByCategoriaIdAsync_SinRelaciones_RetornaVacio()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetCMByCategoriaIdAsync(categoria.Id);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCMByMarcaIdAsync_Exito()
        {
            using var context = GetDbContext();
            var categoria = new Categoria { Nombre = "Electrónica" };
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Categorias.Add(categoria);
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var cm = new CategoriaMarca { CategoriaId = categoria.Id, MarcaId = marca.Id };
            context.CategoriasMarcas.Add(cm);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetCMByMarcaIdAsync(marca.Id);

            Assert.NotNull(result);
            var cmDto = Assert.Single(result);
            Assert.Equal(categoria.Id, cmDto.CategoriaId);
            Assert.Equal(marca.Id, cmDto.MarcaId);
        }

        [Fact]
        public async Task GetCMByMarcaIdAsync_MarcaNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCMByMarcaIdAsync(999));
        }

        [Fact]
        public async Task GetCMByMarcaIdAsync_SinRelaciones_RetornaVacio()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Japón" };
            var marca = new Marca { Nombre = "Sony", Pais = pais };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetCMByMarcaIdAsync(marca.Id);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCMAsync_Paginado_RetornaCorrecto()
        {
            using var context = GetDbContext();
            var categoria1 = new Categoria { Nombre = "Electrónica" };
            var categoria2 = new Categoria { Nombre = "Hogar" };
            var pais = new Pais { Nombre = "Japón" };
            var marca1 = new Marca { Nombre = "Sony", Pais = pais };
            var marca2 = new Marca { Nombre = "Panasonic", Pais = pais };
            context.Categorias.AddRange(categoria1, categoria2);
            context.Paises.Add(pais);
            context.Marcas.AddRange(marca1, marca2);
            await context.SaveChangesAsync();

            context.CategoriasMarcas.AddRange(
                new CategoriaMarca { CategoriaId = categoria1.Id, MarcaId = marca1.Id },
                new CategoriaMarca { CategoriaId = categoria2.Id, MarcaId = marca2.Id }
            );
            await context.SaveChangesAsync();

            var service = GetService(context);

            var (result, total) = await service.GetCMAsync(1, 1, null);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, total);
        }
    }
}