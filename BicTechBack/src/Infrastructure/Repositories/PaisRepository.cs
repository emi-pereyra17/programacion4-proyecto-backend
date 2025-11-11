using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Repositories;

namespace BicTechBack.src.Infrastructure.Repositories
{
    public class PaisRepository : Repository<Pais>, IPaisRepository
    {
        public PaisRepository(AppDbContext context) : base(context) { }

        public override async Task<IEnumerable<Pais>> GetAllAsync()
        {
            return await _context.Paises
                .Include(p => p.Marcas)
                .ToListAsync();
        }

        public override async Task<Pais?> GetByIdAsync(int id)
        {
            return await _context.Paises
                .Include(p => p.Marcas)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}