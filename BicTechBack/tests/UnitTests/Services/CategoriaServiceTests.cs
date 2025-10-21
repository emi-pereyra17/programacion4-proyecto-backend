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
    public class CategoriaServiceTests
    {
        [Fact]
        public async Task CreateCategoriaAsync_CategoriaValida_CreaCategoriaYRetornaDTO()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            var dto = new CrearCategoriaDTO { Nombre = "Electrónica" };
            var categoria = new Categoria { Id = 1, Nombre = "Electrónica" };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Categoria>());
            mockMapper.Setup(m => m.Map<Categoria>(dto)).Returns(categoria);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Categoria>())).ReturnsAsync(categoria);
            mockMapper.Setup(m => m.Map<CategoriaDTO>(categoria)).Returns(new CategoriaDTO { Id = 1, Nombre = "Electrónica" });

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.CreateCategoriaAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Nombre, result.Nombre);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Categoria>()), Times.Once);
        }

        [Fact]
        public async Task CreateCategoriaAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            var dto = new CrearCategoriaDTO { Nombre = "Electrónica" };
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Categoria> { new Categoria { Id = 1, Nombre = "Electrónica" } });

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCategoriaAsync(dto));
        }

        [Fact]
        public async Task DeleteCategoriaAsync_EliminacionCorrecta_RetornaTrue()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.DeleteCategoriaAsync(1);

            Assert.True(result);
            mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoriaAsync_CategoriaNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteCategoriaAsync(1));
        }

        [Fact]
        public async Task GetAllCategoriasAsync_CategoriasExistentes_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            var categorias = new List<Categoria>
            {
                new Categoria { Id = 1, Nombre = "Electrónica" },
                new Categoria { Id = 2, Nombre = "Ropa" }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(categorias);
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaDTO>>(It.IsAny<IEnumerable<Categoria>>()))
                .Returns(new List<CategoriaDTO>
                {
                    new CategoriaDTO { Id = 1, Nombre = "Electrónica" },
                    new CategoriaDTO { Id = 2, Nombre = "Ropa" }
                });

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetAllCategoriasAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllCategoriasAsync_NoCategorias_RetornaListaVacia()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Categoria>());
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaDTO>>(It.IsAny<IEnumerable<Categoria>>()))
                .Returns(new List<CategoriaDTO>());

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetAllCategoriasAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCategoriaByIdAsync_CategoriaExistente_RetornaDTO()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            var categoria = new Categoria { Id = 1, Nombre = "Electrónica" };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(categoria);
            mockMapper.Setup(m => m.Map<CategoriaDTO>(It.IsAny<Categoria>())).Returns(new CategoriaDTO { Id = 1, Nombre = "Electrónica" });

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetCategoriaByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetCategoriaByIdAsync_CategoriaNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Categoria?)null);

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCategoriaByIdAsync(1));
        }

        [Fact]
        public async Task GetCategoriasAsync_CategoriasExistentesYPaginacion_RetornaListaDeDTOs()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            var categorias = new List<Categoria>
            {
                new Categoria { Id = 1, Nombre = "Electrónica" },
                new Categoria { Id = 2, Nombre = "Ropa" },
                new Categoria { Id = 3, Nombre = "Hogar" }
            };

            var page = 1;
            var pageSize = 2;
            var total = categorias.Count();

            var categoriasPaginadas = categorias.Skip((page - 1) * pageSize).Take(pageSize);

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(categorias);
            mockMapper.Setup(m => m.Map<IEnumerable<CategoriaDTO>>(It.IsAny<IEnumerable<Categoria>>()))
                .Returns(categoriasPaginadas.Select(c => new CategoriaDTO { Id = c.Id, Nombre = c.Nombre }));

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.GetCategoriasAsync(page, pageSize, null);

            Assert.NotNull(result);
            Assert.Equal(2, result.Categorias.Count());
            Assert.Equal(total, result.Total);
        }

        [Fact]
        public async Task UpdateCategoriaAsync_CategoriaActualizada_RetornaDTO()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            var categoriaExistente = new Categoria { Id = 1, Nombre = "Electrónica" };
            var dto = new CrearCategoriaDTO { Nombre = "Ropa" };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(categoriaExistente);
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Categoria> { categoriaExistente });
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Categoria>())).ReturnsAsync(categoriaExistente);
            mockMapper.Setup(m => m.Map<CategoriaDTO>(It.IsAny<Categoria>())).Returns(new CategoriaDTO { Id = 1, Nombre = "Ropa" });

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            var result = await service.UpdateCategoriaAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal("Ropa", result.Nombre);
        }

        [Fact]
        public async Task UpdateCategoriaAsync_CategoriaNoExistente_LanzaKeyNotFoundException()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            var dto = new CrearCategoriaDTO { Nombre = "Ropa" };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Categoria?)null);

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateCategoriaAsync(1, dto));
        }

        [Fact]
        public async Task UpdateCategoriaAsync_NombreDuplicado_LanzaInvalidOperationException()
        {
            var mockRepo = new Mock<ICategoriaRepository>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoriaService>>();

            var categoriaExistente = new Categoria { Id = 1, Nombre = "Electrónica" };
            var dto = new CrearCategoriaDTO { Nombre = "Ropa" };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(categoriaExistente);
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Categoria>
            {
                categoriaExistente,
                new Categoria { Id = 2, Nombre = "Ropa" }
            });

            var service = new CategoriaService(mockRepo.Object, mockMapper.Object, mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateCategoriaAsync(1, dto));
        }
    }
}