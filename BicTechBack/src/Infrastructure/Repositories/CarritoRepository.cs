using BicTechBack.src.Core.Entities;
using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BicTechBack.src.Infrastructure.Repositories
{
    public class CarritoRepository : ICarritoRepository
    {
        private readonly AppDbContext _context;

        public CarritoRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Carrito> AddAsync(Carrito entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Carrito> AddProductoAsync(int usuarioId, int productoId, int cantidad)
        {
            var carrito = await _context.Carritos
                .Include(c => c.CarritosDetalles)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    UsuarioId = usuarioId,
                    ActualizadoEn = DateTime.UtcNow,
                    CarritosDetalles = new List<CarritoDetalle>()
                };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            var detalle = carrito.CarritosDetalles?.FirstOrDefault(cd => cd.ProductoId == productoId);

            if (detalle != null)
            {
                detalle.Cantidad += cantidad;
                _context.CarritosDetalles.Update(detalle);
            }
            else
            {
                detalle = new CarritoDetalle
                {
                    CarritoId = carrito.Id,
                    ProductoId = productoId,
                    Cantidad = cantidad
                };
                _context.CarritosDetalles.Add(detalle);
            }

            carrito.ActualizadoEn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return carrito;
        }

        public async Task<Carrito> ClearAsync(int usuarioId)
        {
            var carrito = await _context.Carritos           
                .Include(c => c.CarritosDetalles)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrito == null)
            {
                return null;
            }

            _context.CarritosDetalles.RemoveRange(carrito.CarritosDetalles);

            carrito.ActualizadoEn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _context.Entry(carrito).Collection(c => c.CarritosDetalles).LoadAsync();
            return carrito;
        }

        public async Task<Carrito> DeleteAsync(int usuarioId, int productoId)
        {
            var carrito = await _context.Carritos
                .Include(c => c.CarritosDetalles)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrito == null)
            {
                return null;
            }

            var detalle = carrito.CarritosDetalles?.FirstOrDefault(cd => cd.ProductoId == productoId);

            if (detalle == null)
            {
                return null;
            }
            else 
            {
                _context.CarritosDetalles.Remove(detalle);
                carrito.ActualizadoEn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            await _context.Entry(carrito).Collection(c => c.CarritosDetalles).LoadAsync();
            return carrito;
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Carrito>> GetAllAsync()
        {
            return await _context.Carritos
                .Include(c => c.CarritosDetalles)
                .ThenInclude(cd => cd.Producto)
                .ToListAsync();
        }

        public Task<Carrito?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Carrito?> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Carritos
                .Include(c => c.CarritosDetalles)
                .ThenInclude(cd => cd.Producto)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        }

        public async Task<Carrito> UpdateAsync(int usuarioId, int productoId, int cantidad)
        {
            var carrito = await _context.Carritos
                .Include(c => c.CarritosDetalles)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (carrito == null)
                return null;

            var detalle = carrito.CarritosDetalles?.FirstOrDefault(cd => cd.ProductoId == productoId);

            if (detalle == null)
            {
                return null;
            }
            else
            {
                detalle.Cantidad = cantidad;
                carrito.ActualizadoEn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await _context.Entry(carrito).Collection(c => c.CarritosDetalles).LoadAsync();
                return carrito; 
            }
        }

        public Task<Carrito> UpdateAsync(Carrito entity)
        {
            throw new NotImplementedException();
        }
    }
}
