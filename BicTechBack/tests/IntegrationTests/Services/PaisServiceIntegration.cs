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
    public class PaisServiceIntegration
    {
        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
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

        private PaisService GetService(AppDbContext context)
        {
            var repo = new PaisRepository(context);
            var mapper = GetMapper();
            var logger = new LoggerFactory().CreateLogger<PaisService>();
            return new PaisService(repo, mapper, logger);
        }

        [Fact]
        public async Task CreatePaisAsync_Valido_CreaPais()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var dto = new CrearPaisDTO { Nombre = "Argentina" };
            var result = await service.CreatePaisAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Argentina", result.Nombre);
            Assert.True(result.Id > 0);

            var enDb = await context.Paises.FirstOrDefaultAsync(p => p.Nombre == "Argentina");
            Assert.NotNull(enDb);
        }

        [Fact]
        public async Task CreatePaisAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            context.Paises.Add(new Pais { Nombre = "Argentina" });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearPaisDTO { Nombre = "Argentina" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePaisAsync(dto));
        }

        [Fact]
        public async Task DeletePaisAsync_Exito_EliminaPais()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            context.Paises.Add(pais);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.DeletePaisAsync(pais.Id);

            Assert.True(result);
            Assert.False(context.Paises.Any(p => p.Id == pais.Id));
        }

        [Fact]
        public async Task DeletePaisAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeletePaisAsync(999));
        }

        [Fact]
        public async Task GetAllPaisesAsync_ExistenPaises_RetornaLista()
        {
            using var context = GetDbContext();
            context.Paises.AddRange(
                new Pais { Nombre = "Argentina" },
                new Pais { Nombre = "Brasil" }
            );
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetAllPaisesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllPaisesAsync_SinPaises_RetornaVacio()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllPaisesAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPaisByIdAsync_Exito()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            context.Paises.Add(pais);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetPaisByIdAsync(pais.Id);

            Assert.NotNull(result);
            Assert.Equal(pais.Id, result.Id);
            Assert.Equal("Argentina", result.Nombre);
        }

        [Fact]
        public async Task GetPaisByIdAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetPaisByIdAsync(999));
        }

        [Fact]
        public async Task GetPaisesAsync_Paginado_RetornaCorrecto()
        {
            using var context = GetDbContext();
            context.Paises.AddRange(
                new Pais { Nombre = "Argentina" },
                new Pais { Nombre = "Brasil" }
            );
            await context.SaveChangesAsync();

            var service = GetService(context);

            var (result, total) = await service.GetPaisesAsync(1, 1, null);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, total);
        }

        [Fact]
        public async Task UpdatePaisAsync_Exito_ActualizaPais()
        {
            using var context = GetDbContext();
            var pais = new Pais { Nombre = "Argentina" };
            context.Paises.Add(pais);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearPaisDTO { Nombre = "Uruguay" };

            var result = await service.UpdatePaisAsync(pais.Id, dto);

            Assert.NotNull(result);
            Assert.Equal("Uruguay", result.Nombre);

            var enDb = await context.Paises.FindAsync(pais.Id);
            Assert.Equal("Uruguay", enDb.Nombre);
        }

        [Fact]
        public async Task UpdatePaisAsync_NoExiste_LanzaKeyNotFoundException()
        {
            using var context = GetDbContext();
            var service = GetService(context);
            var dto = new CrearPaisDTO { Nombre = "Nueva" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdatePaisAsync(999, dto));
        }

        [Fact]
        public async Task UpdatePaisAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            using var context = GetDbContext();
            var pais1 = new Pais { Nombre = "Argentina" };
            var pais2 = new Pais { Nombre = "Brasil" };
            context.Paises.AddRange(pais1, pais2);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var dto = new CrearPaisDTO { Nombre = "Brasil" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdatePaisAsync(pais1.Id, dto));
        }
    }
}