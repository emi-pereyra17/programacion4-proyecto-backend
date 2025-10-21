using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BicTechBack.src.Infrastructure.Repositories
{
    public class PedidoDetalleRepository : IPedidoDetalleRepository
    {
        private readonly AppDbContext _context;

        public PedidoDetalleRepository(AppDbContext context)
        {
            _context = context;   
        }
        public async Task<PedidoDetalle> AddAsync(PedidoDetalle pedidoDetalle)
        {
            _context.PedidosDetalles.Add(pedidoDetalle);
            await _context.SaveChangesAsync();
            return pedidoDetalle;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pedidoDetalle = await _context.PedidosDetalles.FindAsync(id);
            if (pedidoDetalle == null)
            {
                return false;
            }
            _context.PedidosDetalles.Remove(pedidoDetalle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PedidoDetalle>> GetAllAsync()
        {
           return await _context.PedidosDetalles
                .Include(pd => pd.Pedido)
                .Include(pd => pd.Producto)
                .ToListAsync();
        }

        public async Task<PedidoDetalle?> GetByIdAsync(int id)
        {
            return await _context.PedidosDetalles
                .Include(pd => pd.Pedido)
                .Include(pd => pd.Producto)
                .FirstOrDefaultAsync(pd => pd.Id == id);
        }

        public async Task<PedidoDetalle> UpdateAsync(PedidoDetalle pedidoDetalle)
        {
            _context.PedidosDetalles.Update(pedidoDetalle);
            await _context.SaveChangesAsync();
            return pedidoDetalle;
        }
    }
}
