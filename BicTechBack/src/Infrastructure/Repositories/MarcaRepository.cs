using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Repositories;

namespace BicTechBack.src.Infrastructure.Repositories
{
    public class MarcaRepository : Repository<Marca>, IMarcaRepository
    {
        public MarcaRepository(AppDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Marcas.AnyAsync(m => m.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null)
        {
            return await _context.Marcas
                .AnyAsync(m => m.Nombre.ToLower() == nombre.ToLower() && (!excludeId.HasValue || m.Id != excludeId.Value));
        }

        public override async Task<IEnumerable<Marca>> GetAllAsync()
        {
            return await _context.Marcas
                .Include(m => m.Pais)
                .ToListAsync();
        }

        public override async Task<Marca?> GetByIdAsync(int id)
        {
            return await _context.Marcas
                .Include(m => m.Pais)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}