using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BicTechBack.src.Infrastructure.Repositories
{
    public class CategoriaMarcaRepository : ICategoriaMarcaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaMarcaRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<CategoriaMarca> AddAsync(CategoriaMarca categoriaMarca)
        {
            _context.CategoriasMarcas.Add(categoriaMarca);
            await _context.SaveChangesAsync();
            return categoriaMarca;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var categoriaMarca = await _context.CategoriasMarcas.FindAsync(id);
            if (categoriaMarca == null)
            {
                return false;
            }

            _context.CategoriasMarcas.Remove(categoriaMarca);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CategoriaMarca>> GetAllAsync()
        {
            return await _context.CategoriasMarcas
                .Include(cm => cm.Categoria)
                .Include(cm => cm.Marca)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoriaMarca>> GetByCategoriaIdAsync(int categoriaId)
        {
            return await _context.CategoriasMarcas
                .Where(cm => cm.CategoriaId == categoriaId)
                .Include(cm => cm.Categoria)
                .Include(cm => cm.Marca)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoriaMarca>> GetByMarcaIdAsync(int marcaId)
        {
            return await _context.CategoriasMarcas
                .Where(cm => cm.MarcaId == marcaId)
                .Include(cm => cm.Categoria)
                .Include(cm => cm.Marca)
                .ToListAsync();
        }
    }
}
