using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BicTechBack.UnitTests.Services
{
    public class PaisServiceTests
    {
        [Fact]
        public async Task CreatePaisAsync_PaisValido_CreaPaisYRetornaDTO()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            var dto = new CrearPaisDTO { Nombre = "Argentina" };
            var pais = new Pais { Id = 1, Nombre = "Argentina" };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Pais>());
            mockMapper.Setup(m => m.Map<Pais>(dto)).Returns(pais);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Pais>())).ReturnsAsync(pais);
            mockMapper.Setup(m => m.Map<PaisDTO>(pais)).Returns(new PaisDTO { Id = 1, Nombre = "Argentina" });

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.CreatePaisAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Nombre, result.Nombre);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Pais>()), Times.Once);
        }

        [Fact]
        public async Task CreatePaisAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            var dto = new CrearPaisDTO { Nombre = "Argentina" };
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Pais> { new Pais { Id = 1, Nombre = "Argentina" } });

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePaisAsync(dto));
        }

        [Fact]
        public async Task DeletePaisAsync_EliminacionCorrecta_RetornaTrue()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.DeletePaisAsync(1);

            Assert.True(result);
            mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeletePaisAsync_PaisNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeletePaisAsync(1));
        }

        [Fact]
        public async Task GetAllPaisesAsync_PaisesExistentes_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            var paises = new List<Pais>
            {
                new Pais { Id = 1, Nombre = "Argentina" },
                new Pais { Id = 2, Nombre = "Brasil" }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(paises);
            mockMapper.Setup(m => m.Map<IEnumerable<PaisDTO>>(It.IsAny<IEnumerable<Pais>>()))
                .Returns(new List<PaisDTO>
                {
                    new PaisDTO { Id = 1, Nombre = "Argentina" },
                    new PaisDTO { Id = 2, Nombre = "Brasil" }
                });

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetAllPaisesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllPaisesAsync_NoPaises_RetornaListaVacia()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Pais>());
            mockMapper.Setup(m => m.Map<IEnumerable<PaisDTO>>(It.IsAny<IEnumerable<Pais>>()))
                .Returns(new List<PaisDTO>());

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetAllPaisesAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPaisByIdAsync_PaisExistente_RetornaDTO()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            var pais = new Pais { Id = 1, Nombre = "Argentina" };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pais);
            mockMapper.Setup(m => m.Map<PaisDTO>(It.IsAny<Pais>())).Returns(new PaisDTO { Id = 1, Nombre = "Argentina" });

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetPaisByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetPaisByIdAsync_PaisNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Pais?)null);

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetPaisByIdAsync(1));
        }

        [Fact]
        public async Task GetPaisesAsync_PaisesExistentesYPaginacion_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            var paises = new List<Pais>
            {
                new Pais { Id = 1, Nombre = "Argentina" },
                new Pais { Id = 2, Nombre = "Brasil" },
                new Pais { Id = 3, Nombre = "Chile" }
            };

            var page = 1;
            var pageSize = 2;
            var total = paises.Count();

            var paisesPaginados = paises.Skip((page - 1) * pageSize).Take(pageSize);

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(paises);
            mockMapper.Setup(m => m.Map<IEnumerable<PaisDTO>>(It.IsAny<IEnumerable<Pais>>()))
                .Returns(paisesPaginados.Select(p => new PaisDTO { Id = p.Id, Nombre = p.Nombre }));

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetPaisesAsync(page, pageSize, null);

            Assert.NotNull(result);
            Assert.Equal(2, result.Paises.Count());
            Assert.Equal(total, result.Total);
        }

        [Fact]
        public async Task UpdatePaisAsync_PaisActualizado_RetornaDTO()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            var paisExistente = new Pais { Id = 1, Nombre = "Argentina" };
            var dto = new CrearPaisDTO { Nombre = "Uruguay" };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(paisExistente);
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Pais> { paisExistente });
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Pais>())).ReturnsAsync(paisExistente);
            mockMapper.Setup(m => m.Map<PaisDTO>(It.IsAny<Pais>())).Returns(new PaisDTO { Id = 1, Nombre = "Uruguay" });

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.UpdatePaisAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal("Uruguay", result.Nombre);
        }

        [Fact]
        public async Task UpdatePaisAsync_PaisNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            var dto = new CrearPaisDTO { Nombre = "Uruguay" };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Pais?)null);
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Pais>());

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdatePaisAsync(1, dto));
        }

        [Fact]
        public async Task UpdatePaisAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<IAppLogger<PaisService>>();

            var paisExistente = new Pais { Id = 1, Nombre = "Argentina" };
            var dto = new CrearPaisDTO { Nombre = "Brasil" };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(paisExistente);
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Pais>
            {
                paisExistente,
                new Pais { Id = 2, Nombre = "Brasil" }
            });

            var service = new PaisService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdatePaisAsync(1, dto));
        }
    }
}