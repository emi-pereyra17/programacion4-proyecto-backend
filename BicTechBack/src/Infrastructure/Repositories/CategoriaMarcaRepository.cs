using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Repositories;

namespace BicTechBack.src.Infrastructure.Repositories
{
    public class CategoriaMarcaRepository : Repository<CategoriaMarca>, ICategoriaMarcaRepository
    {
        public CategoriaMarcaRepository(AppDbContext context) : base(context) { }

        public override async Task<IEnumerable<CategoriaMarca>> GetAllAsync()
        {
            return await _context.CategoriasMarcas
                .Include(cm => cm.Categoria)
                .Include(cm => cm.Marca)
                .ToListAsync();
        }

        public override async Task<CategoriaMarca?> GetByIdAsync(int id)
        {
            return await _context.CategoriasMarcas
                .Include(cm => cm.Categoria)
                .Include(cm => cm.Marca)
                .FirstOrDefaultAsync(cm => cm.Id == id);
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