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
    public class MarcaServiceIntegration
    {
        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MarcaProfile>();
                cfg.AddProfile<PaisProfile>();
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

        private MarcaService GetService(AppDbContext context)
        {
            var repo = new MarcaRepository(context);
            var paisRepo = new PaisRepository(context);
            var mapper = GetMapper();
            var msLogger = new LoggerFactory().CreateLogger<MarcaService>();
            var logger = new LoggerAdapter<MarcaService>(msLogger);
            return new MarcaService(repo, mapper, logger, paisRepo);
        }

        [Fact]
        public async Task CreateMarcaAsync_Valido_CreaMarca()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            context.Paises.Add(pais);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearMarcaDTO { Nombre = "Nike", PaisId = pais.Id };

            var result = await service.CreateMarcaAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Nike", result.Nombre);
            Assert.Equal("Argentina", result.Pais);
            Assert.True(result.Id > 0);

            var enDb = await context.Marcas.FirstOrDefaultAsync(m => m.Nombre == "Nike");
            Assert.NotNull(enDb);
        }

        [Fact]
        public async Task CreateMarcaAsync_PaisNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);
            var dto = new CrearMarcaDTO { Nombre = "Nike", PaisId = 999 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateMarcaAsync(dto));
        }

        [Fact]
        public async Task CreateMarcaAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            var marca = new Marca { Nombre = "Nike", Pais = pais };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearMarcaDTO { Nombre = "Nike", PaisId = pais.Id };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateMarcaAsync(dto));
        }

        [Fact]
        public async Task DeleteMarcaAsync_Exito_EliminaMarca()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            var marca = new Marca { Nombre = "Nike", Pais = pais };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.DeleteMarcaAsync(marca.Id);

            Assert.True(result);
            Assert.False(context.Marcas.Any(m => m.Id == marca.Id));
        }

        [Fact]
        public async Task DeleteMarcaAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteMarcaAsync(999));
        }

        [Fact]
        public async Task GetAllMarcasAsync_ExistenMarcas_RetornaLista()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            context.Paises.Add(pais);
            context.Marcas.AddRange(
                new Marca { Nombre = "Nike", Pais = pais },
                new Marca { Nombre = "Adidas", Pais = pais }
            );
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetAllMarcasAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllMarcasAsync_SinMarcas_RetornaVacio()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllMarcasAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMarcaByIdAsync_Exito()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            var marca = new Marca { Nombre = "Nike", Pais = pais };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetMarcaByIdAsync(marca.Id);

            Assert.NotNull(result);
            Assert.Equal(marca.Id, result.Id);
            Assert.Equal("Nike", result.Nombre);
            Assert.Equal("Argentina", result.Pais);
        }

        [Fact]
        public async Task GetMarcaByIdAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetMarcaByIdAsync(999));
        }

        [Fact]
        public async Task GetMarcasAsync_Paginado_RetornaCorrecto()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            context.Paises.Add(pais);
            context.Marcas.AddRange(
                new Marca { Nombre = "Nike", Pais = pais },
                new Marca { Nombre = "Adidas", Pais = pais }
            );
            await context.SaveChangesAsync();

            var service = GetService(context);

            var (result, total) = await service.GetMarcasAsync(1, 1, null);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, total);
        }

        [Fact]
        public async Task UpdateMarcaAsync_Exito_ActualizaMarca()
        {
            using var context = GetDbContext();
            var pais1 = new Pais { Nombre = "Argentina" };
            var pais2 = new Pais { Nombre = "Brasil" };
            var marca = new Marca { Nombre = "Nike", Pais = pais1 };
            context.Paises.AddRange(pais1, pais2);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearMarcaDTO { Nombre = "Adidas", PaisId = pais2.Id };

            var result = await service.UpdateMarcaAsync(marca.Id, dto);

            Assert.NotNull(result);
            Assert.Equal("Adidas", result.Nombre);
            Assert.Equal("Brasil", result.Pais);

            var enDb = await context.Marcas.FindAsync(marca.Id);
            Assert.Equal("Adidas", enDb.Nombre);
            Assert.Equal(pais2.Id, enDb.PaisId);
        }

        [Fact]
        public async Task UpdateMarcaAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            context.Paises.Add(pais);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearMarcaDTO { Nombre = "Adidas", PaisId = pais.Id };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateMarcaAsync(999, dto));
        }

        [Fact]
        public async Task UpdateMarcaAsync_PaisNoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            var marca = new Marca { Nombre = "Nike", Pais = pais };
            context.Paises.Add(pais);
            context.Marcas.Add(marca);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearMarcaDTO { Nombre = "Adidas", PaisId = 999 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateMarcaAsync(marca.Id, dto));
        }

        [Fact]
        public async Task UpdateMarcaAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            var marca1 = new Marca { Nombre = "Nike", Pais = pais };
            var marca2 = new Marca { Nombre = "Adidas", Pais = pais };
            context.Paises.Add(pais);
            context.Marcas.AddRange(marca1, marca2);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearMarcaDTO { Nombre = "Adidas", PaisId = pais.Id };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateMarcaAsync(marca1.Id, dto));
        }
    }
}