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
    public class CategoriaMarcaServiceTests
    {
        [Fact]
        public async Task CreateCMAsync_Valido_CreaRelacionYRetornaDTO()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var dto = new CrearCategoriaMarcaDTO { CategoriaId = 1, MarcaId = 2 };
            var categoria = new Categoria { Id = 1, Nombre = "Electrónica" };
            var marca = new Marca { Id = 2, Nombre = "Nike" };
            var cm = new CategoriaMarca { Id = 1, CategoriaId = 1, MarcaId = 2, Categoria = categoria, Marca = marca };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CategoriaMarca>());
            mockCategoriaRepo.Setup(r => r.GetByIdAsync(dto.CategoriaId)).ReturnsAsync(categoria);
            mockMarcaRepo.Setup(r => r.GetByIdAsync(dto.MarcaId)).ReturnsAsync(marca);
            mockMapper.Setup(m => m.Map<CategoriaMarca>(dto)).Returns(cm);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<CategoriaMarca>())).ReturnsAsync(cm);
            mockMapper.Setup(m => m.Map<CategoriaMarcaDTO>(cm)).Returns(new CategoriaMarcaDTO { Id = 1, CategoriaId = 1, MarcaId = 2 });

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.CreateCMAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(1, result.CategoriaId);
            Assert.Equal(2, result.MarcaId);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<CategoriaMarca>()), Times.Once);
        }

        [Fact]
        public async Task CreateCMAsync_CategoriaNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var dto = new CrearCategoriaMarcaDTO { CategoriaId = 1, MarcaId = 2 };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CategoriaMarca>());
            mockCategoriaRepo.Setup(r => r.GetByIdAsync(dto.CategoriaId)).ReturnsAsync((Categoria?)null);

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateCMAsync(dto));
        }

        [Fact]
        public async Task CreateCMAsync_MarcaNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var dto = new CrearCategoriaMarcaDTO { CategoriaId = 1, MarcaId = 2 };
            var categoria = new Categoria { Id = 1, Nombre = "Electrónica" };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CategoriaMarca>());
            mockCategoriaRepo.Setup(r => r.GetByIdAsync(dto.CategoriaId)).ReturnsAsync(categoria);
            mockMarcaRepo.Setup(r => r.GetByIdAsync(dto.MarcaId)).ReturnsAsync((Marca?)null);

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateCMAsync(dto));
        }

        [Fact]
        public async Task CreateCMAsync_RelacionDuplicada_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var dto = new CrearCategoriaMarcaDTO { CategoriaId = 1, MarcaId = 2 };
            var categoria = new Categoria { Id = 1, Nombre = "Electrónica" };
            var marca = new Marca { Id = 2, Nombre = "Nike" };
            var existente = new CategoriaMarca { Id = 1, CategoriaId = 1, MarcaId = 2, Categoria = categoria, Marca = marca };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CategoriaMarca> { existente });
            mockCategoriaRepo.Setup(r => r.GetByIdAsync(dto.CategoriaId)).ReturnsAsync(categoria);
            mockMarcaRepo.Setup(r => r.GetByIdAsync(dto.MarcaId)).ReturnsAsync(marca);

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCMAsync(dto));
        }

        [Fact]
        public async Task DeleteCMAsync_EliminacionCorrecta_RetornaTrue()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.DeleteCMAsync(1);

            Assert.True(result);
            mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteCMAsync_NoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteCMAsync(1));
        }

        [Fact]
        public async Task GetAllCMAsync_ExistenRelaciones_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var relaciones = new List<CategoriaMarca>
            {
                new CategoriaMarca { Id = 1, CategoriaId = 1, MarcaId = 2 },
                new CategoriaMarca { Id = 2, CategoriaId = 2, MarcaId = 3 }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(relaciones);
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaMarcaDTO>>(It.IsAny<IEnumerable<CategoriaMarca>>()))
                .Returns(new List<CategoriaMarcaDTO>
                {
                    new CategoriaMarcaDTO { Id = 1, CategoriaId = 1, MarcaId = 2 },
                    new CategoriaMarcaDTO { Id = 2, CategoriaId = 2, MarcaId = 3 }
                });

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetAllCMAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllCMAsync_NoRelaciones_RetornaListaVacia()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CategoriaMarca>());
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaMarcaDTO>>(It.IsAny<IEnumerable<CategoriaMarca>>()))
                .Returns(new List<CategoriaMarcaDTO>());

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetAllCMAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMarcasPorCategoriaAsync_CategoriaExistente_RetornaMarcas()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var categoria = new Categoria { Id = 1, Nombre = "Electrónica" };
            var marca = new Marca { Id = 2, Nombre = "Nike" };
            var relaciones = new List<CategoriaMarca>
            {
                new CategoriaMarca { Id = 1, CategoriaId = 1, MarcaId = 2, Marca = marca }
            };

            mockCategoriaRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(categoria);
            mockRepo.Setup(r => r.GetByCategoriaIdAsync(1)).ReturnsAsync(relaciones);
            mockMapper.Setup(m => m.Map<IEnumerable<MarcaDTO>>(It.IsAny<IEnumerable<Marca>>()))
                .Returns(new List<MarcaDTO> { new MarcaDTO { Id = 2, Nombre = "Nike" } });

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetMarcasPorCategoriaAsync(1);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, result.First().Id);
        }

        [Fact]
        public async Task GetMarcasPorCategoriaAsync_CategoriaNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            mockCategoriaRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Categoria?)null);

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetMarcasPorCategoriaAsync(1));
        }

        [Fact]
        public async Task GetCategoriasPorMarcaAsync_MarcaExistente_RetornaCategorias()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var marca = new Marca { Id = 2, Nombre = "Nike" };
            var categoria = new Categoria { Id = 1, Nombre = "Electrónica" };
            var relaciones = new List<CategoriaMarca>
            {
                new CategoriaMarca { Id = 1, CategoriaId = 1, MarcaId = 2, Categoria = categoria }
            };

            mockMarcaRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(marca);
            mockRepo.Setup(r => r.GetByMarcaIdAsync(2)).ReturnsAsync(relaciones);
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaDTO>>(It.IsAny<IEnumerable<Categoria>>()))
                .Returns(new List<CategoriaDTO> { new CategoriaDTO { Id = 1, Nombre = "Electrónica" } });

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetCategoriasPorMarcaAsync(2);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public async Task GetCategoriasPorMarcaAsync_MarcaNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            mockMarcaRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Marca?)null);

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCategoriasPorMarcaAsync(2));
        }

        [Fact]
        public async Task GetCMByCategoriaIdAsync_CategoriaExistente_RetornaRelaciones()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var categoria = new Categoria { Id = 1, Nombre = "Electrónica" };
            var relaciones = new List<CategoriaMarca>
            {
                new CategoriaMarca { Id = 1, CategoriaId = 1, MarcaId = 2 }
            };

            mockCategoriaRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(categoria);
            mockRepo.Setup(r => r.GetByCategoriaIdAsync(1)).ReturnsAsync(relaciones);
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaMarcaDTO>>(It.IsAny<IEnumerable<CategoriaMarca>>()))
                .Returns(new List<CategoriaMarcaDTO> { new CategoriaMarcaDTO { Id = 1, CategoriaId = 1, MarcaId = 2 } });

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetCMByCategoriaIdAsync(1);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().CategoriaId);
        }

        [Fact]
        public async Task GetCMByCategoriaIdAsync_CategoriaNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            mockCategoriaRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Categoria?)null);

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCMByCategoriaIdAsync(1));
        }

        [Fact]
        public async Task GetCMByCategoriaIdAsync_NoRelaciones_RetornaListaVacia()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var categoria = new Categoria { Id = 1, Nombre = "Electrónica" };

            mockCategoriaRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(categoria);
            mockRepo.Setup(r => r.GetByCategoriaIdAsync(1)).ReturnsAsync(new List<CategoriaMarca>());
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaMarcaDTO>>(It.IsAny<IEnumerable<CategoriaMarca>>()))
                .Returns(new List<CategoriaMarcaDTO>());

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetCMByCategoriaIdAsync(1);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCMByMarcaIdAsync_MarcaExistente_RetornaRelaciones()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var marca = new Marca { Id = 2, Nombre = "Nike" };
            var relaciones = new List<CategoriaMarca>
            {
                new CategoriaMarca { Id = 1, CategoriaId = 1, MarcaId = 2 }
            };

            mockMarcaRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(marca);
            mockRepo.Setup(r => r.GetByMarcaIdAsync(2)).ReturnsAsync(relaciones);
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaMarcaDTO>>(It.IsAny<IEnumerable<CategoriaMarca>>()))
                .Returns(new List<CategoriaMarcaDTO> { new CategoriaMarcaDTO { Id = 1, CategoriaId = 1, MarcaId = 2 } });

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetCMByMarcaIdAsync(2);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, result.First().MarcaId);
        }

        [Fact]
        public async Task GetCMByMarcaIdAsync_MarcaNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            mockMarcaRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Marca?)null);

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCMByMarcaIdAsync(2));
        }

        [Fact]
        public async Task GetCMByMarcaIdAsync_NoRelaciones_RetornaListaVacia()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var marca = new Marca { Id = 2, Nombre = "Nike" };

            mockMarcaRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(marca);
            mockRepo.Setup(r => r.GetByMarcaIdAsync(2)).ReturnsAsync(new List<CategoriaMarca>());
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaMarcaDTO>>(It.IsAny<IEnumerable<CategoriaMarca>>()))
                .Returns(new List<CategoriaMarcaDTO>());

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetCMByMarcaIdAsync(2);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCMAsync_ExistenRelacionesYPaginacion_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<ICategoriaMarcaRepository>();
            var mockCategoriaRepo = new Mock<ICategoriaRepository>();
            var mockMarcaRepo = new Mock<IMarcaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaMarcaService>>();

            var relaciones = new List<CategoriaMarca>
            {
                new CategoriaMarca { Id = 1, CategoriaId = 1, MarcaId = 2 },
                new CategoriaMarca { Id = 2, CategoriaId = 2, MarcaId = 3 },
                new CategoriaMarca { Id = 3, CategoriaId = 3, MarcaId = 4 }
            };

            var page = 1;
            var pageSize = 2;
            var total = relaciones.Count();

            var relacionesPaginadas = relaciones.Skip((page - 1) * pageSize).Take(pageSize);

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(relaciones);
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaMarcaDTO>>(It.IsAny<IEnumerable<CategoriaMarca>>()))
                .Returns(relacionesPaginadas.Select(r => new CategoriaMarcaDTO { Id = r.Id, CategoriaId = r.CategoriaId, MarcaId = r.MarcaId }));

            var service = new CategoriaMarcaService(
                mockRepo.Object, mockCategoriaRepo.Object, mockMarcaRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetCMAsync(page, pageSize, null);

            Assert.NotNull(result);
            Assert.Equal(2, result.CategoriasMarcas.Count());
            Assert.Equal(total, result.Total);
        }
    }
}