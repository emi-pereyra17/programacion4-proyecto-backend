using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Repositories;

namespace BicTechBack.src.Infrastructure.Repositories
{
    public class PedidoDetalleRepository : Repository<PedidoDetalle>, IPedidoDetalleRepository
    {
        public PedidoDetalleRepository(AppDbContext context) : base(context) { }

        public override async Task<IEnumerable<PedidoDetalle>> GetAllAsync()
        {
            return await _context.PedidosDetalles
                .Include(pd => pd.Pedido)
                .Include(pd => pd.Producto)
                .ToListAsync();
        }

        public override async Task<PedidoDetalle?> GetByIdAsync(int id)
        {
            return await _context.PedidosDetalles
                .Include(pd => pd.Pedido)
                .Include(pd => pd.Producto)
                .FirstOrDefaultAsync(pd => pd.Id == id);
        }
    }
}