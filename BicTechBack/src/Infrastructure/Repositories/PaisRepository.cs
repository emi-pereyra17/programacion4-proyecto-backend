using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BicTechBack.src.Infrastructure.Repositories
{
    public class PaisRepository : IPaisRepository
    {
        private readonly AppDbContext _context;

        public PaisRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Pais> AddAsync(Pais pais)
        {
            _context.Paises.Add(pais);
            await _context.SaveChangesAsync();
            return pais;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pais = await _context.Paises.FindAsync(id);
            if (pais == null)
                return false;

            _context.Paises.Remove(pais);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Pais>> GetAllAsync()
        {
            return await _context.Paises
                .Include(p => p.Marcas)
                .ToListAsync();
        }

        public async Task<Pais?> GetByIdAsync(int id)
        {
            return await _context.Paises
                .Include(p => p.Marcas)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pais> UpdateAsync(Pais pais)
        {
            _context.Paises.Update(pais);
            await _context.SaveChangesAsync();
            return pais;
        }
    }
}
