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
    public class MarcaServiceTests
    {
        [Fact]
        public async Task CreateMarcaAsync_MarcaValida_CreaMarcaYRetornaDTO()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var dto = new CrearMarcaDTO { Nombre = "Nike", PaisId = 1 };
            var pais = new Pais { Id = 1, Nombre = "Argentina" };
            var marca = new Marca { Id = 1, Nombre = "Nike", PaisId = 1, Pais = pais };

            mockPaisRepo.Setup(r => r.GetByIdAsync(dto.PaisId)).ReturnsAsync(pais);
            mockRepo.Setup(r => r.ExistsByNameAsync(dto.Nombre, null)).ReturnsAsync(false);
            mockMapper.Setup(m => m.Map<Marca>(dto)).Returns(marca);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Marca>())).ReturnsAsync(marca);
            mockMapper.Setup(m => m.Map<MarcaDTO>(marca)).Returns(new MarcaDTO { Id = 1, Nombre = "Nike", Pais = "Argentina" });

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            var result = await service.CreateMarcaAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Nombre, result.Nombre);
            Assert.Equal("Argentina", result.Pais);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Marca>()), Times.Once);
        }

        [Fact]
        public async Task CreateMarcaAsync_PaisNoExiste_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var dto = new CrearMarcaDTO { Nombre = "Nike", PaisId = 1 };

            mockPaisRepo.Setup(r => r.GetByIdAsync(dto.PaisId)).ReturnsAsync((Pais?)null);

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateMarcaAsync(dto));
        }

        [Fact]
        public async Task CreateMarcaAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var dto = new CrearMarcaDTO { Nombre = "Nike", PaisId = 1 };
            var pais = new Pais { Id = 1, Nombre = "Argentina" };

            mockPaisRepo.Setup(r => r.GetByIdAsync(dto.PaisId)).ReturnsAsync(pais);
            mockRepo.Setup(r => r.ExistsByNameAsync(dto.Nombre, null)).ReturnsAsync(true);

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateMarcaAsync(dto));
        }

        [Fact]
        public async Task DeleteMarcaAsync_EliminacionCorrecta_RetornaTrue()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            var result = await service.DeleteMarcaAsync(1);

            Assert.True(result);
            mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteMarcaAsync_MarcaNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteMarcaAsync(1));
        }

        [Fact]
        public async Task GetAllMarcasAsync_MarcasExistentes_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var marcas = new List<Marca>
            {
                new Marca { Id = 1, Nombre = "Nike", PaisId = 1, Pais = new Pais { Id = 1, Nombre = "Argentina" } },
                new Marca { Id = 2, Nombre = "Adidas", PaisId = 2, Pais = new Pais { Id = 2, Nombre = "Brasil" } }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(marcas);
            mockMapper.Setup(m => m.Map<IEnumerable<MarcaDTO>>(It.IsAny<IEnumerable<Marca>>()))
                .Returns(new List<MarcaDTO>
                {
                    new MarcaDTO { Id = 1, Nombre = "Nike", Pais = "Argentina" },
                    new MarcaDTO { Id = 2, Nombre = "Adidas", Pais = "Brasil" }
                });

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            var result = await service.GetAllMarcasAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllMarcasAsync_NoMarcas_RetornaListaVacia()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Marca>());
            mockMapper.Setup(m => m.Map<IEnumerable<MarcaDTO>>(It.IsAny<IEnumerable<Marca>>()))
                .Returns(new List<MarcaDTO>());

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            var result = await service.GetAllMarcasAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMarcaByIdAsync_MarcaExistente_RetornaDTO()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var marca = new Marca { Id = 1, Nombre = "Nike", PaisId = 1, Pais = new Pais { Id = 1, Nombre = "Argentina" } };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(marca);
            mockMapper.Setup(m => m.Map<MarcaDTO>(It.IsAny<Marca>())).Returns(new MarcaDTO { Id = 1, Nombre = "Nike", Pais = "Argentina" });

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            var result = await service.GetMarcaByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetMarcaByIdAsync_MarcaNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Marca?)null);

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetMarcaByIdAsync(1));
        }

        [Fact]
        public async Task GetMarcasAsync_MarcasExistentesYPaginacion_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var marcas = new List<Marca>
            {
                new Marca { Id = 1, Nombre = "Nike", PaisId = 1 },
                new Marca { Id = 2, Nombre = "Adidas", PaisId = 2 },
                new Marca { Id = 3, Nombre = "Puma", PaisId = 3 }
            };

            var page = 1;
            var pageSize = 2;
            var total = marcas.Count();

            var marcasPaginadas = marcas.Skip((page - 1) * pageSize).Take(pageSize);

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(marcas);
            mockMapper.Setup(m => m.Map<IEnumerable<MarcaDTO>>(It.IsAny<IEnumerable<Marca>>()))
                .Returns(marcasPaginadas.Select(m => new MarcaDTO { Id = m.Id, Nombre = m.Nombre, Pais = "Pais" + m.PaisId }));

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            var result = await service.GetMarcasAsync(page, pageSize, null);

            Assert.NotNull(result);
            Assert.Equal(2, result.Marcas.Count());
            Assert.Equal(total, result.Total);
        }

        [Fact]
        public async Task UpdateMarcaAsync_MarcaActualizada_RetornaDTO()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var marcaExistente = new Marca { Id = 1, Nombre = "Nike", PaisId = 1 };
            var pais = new Pais { Id = 2, Nombre = "Brasil" };
            var dto = new CrearMarcaDTO { Nombre = "Adidas", PaisId = 2 };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(marcaExistente);
            mockPaisRepo.Setup(r => r.GetByIdAsync(dto.PaisId)).ReturnsAsync(pais);
            mockRepo.Setup(r => r.ExistsByNameAsync(dto.Nombre, 1)).ReturnsAsync(false);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Marca>())).ReturnsAsync(marcaExistente);
            mockMapper.Setup(m => m.Map<MarcaDTO>(It.IsAny<Marca>())).Returns(new MarcaDTO { Id = 1, Nombre = "Adidas", Pais = "Brasil" });

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            var result = await service.UpdateMarcaAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal("Adidas", result.Nombre);
            Assert.Equal("Brasil", result.Pais);
        }

        [Fact]
        public async Task UpdateMarcaAsync_MarcaNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var dto = new CrearMarcaDTO { Nombre = "Adidas", PaisId = 2 };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Marca?)null);

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateMarcaAsync(1, dto));
        }

        [Fact]
        public async Task UpdateMarcaAsync_PaisNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var marcaExistente = new Marca { Id = 1, Nombre = "Nike", PaisId = 1 };
            var dto = new CrearMarcaDTO { Nombre = "Adidas", PaisId = 2 };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(marcaExistente);
            mockPaisRepo.Setup(r => r.GetByIdAsync(dto.PaisId)).ReturnsAsync((Pais?)null);

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateMarcaAsync(1, dto));
        }

        [Fact]
        public async Task UpdateMarcaAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<IMarcaRepository>();
            var mockPaisRepo = new Mock<IPaisRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<MarcaService>>();

            var marcaExistente = new Marca { Id = 1, Nombre = "Nike", PaisId = 1 };
            var pais = new Pais { Id = 2, Nombre = "Brasil" };
            var dto = new CrearMarcaDTO { Nombre = "Adidas", PaisId = 2 };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(marcaExistente);
            mockPaisRepo.Setup(r => r.GetByIdAsync(dto.PaisId)).ReturnsAsync(pais);
            mockRepo.Setup(r => r.ExistsByNameAsync(dto.Nombre, 1)).ReturnsAsync(true);

            var service = new MarcaService(mockRepo.Object, mockMapper.Object, mockLogger.Object, mockPaisRepo.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateMarcaAsync(1, dto));
        }
    }
}