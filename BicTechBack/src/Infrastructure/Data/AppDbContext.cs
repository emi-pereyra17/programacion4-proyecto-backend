using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidosDetalles { get; set; }
        public DbSet<CarritoDetalle> CarritosDetalles { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet <Pais> Paises { get; set; }
        public DbSet<CategoriaMarca> CategoriasMarcas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Marca)
                .WithMany(m => m.Productos)
                .HasForeignKey(p => p.MarcaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Marca>()
                .HasOne(m => m.Pais)
                .WithMany(p => p.Marcas)
                .HasForeignKey(m => m.PaisId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Carrito>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Carritos)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CarritoDetalle>()
                .HasOne(cd => cd.Carrito)
                .WithMany(c => c.CarritosDetalles)
                .HasForeignKey(cd => cd.CarritoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CarritoDetalle>()
                .HasOne(cd => cd.Producto)
                .WithMany(p => p.CarritosDetalles)
                .HasForeignKey(cd => cd.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PedidoDetalle>()
                .HasOne(pd => pd.Producto)
                .WithMany(p => p.PedidosDetalles)
                .HasForeignKey(pd => pd.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PedidoDetalle>()
                .HasOne(pd => pd.Pedido)
                .WithMany(p => p.PedidosDetalles)
                .HasForeignKey(pd => pd.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PedidoDetalle>()
                .Property(pd => pd.Precio)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PedidoDetalle>()
                .Property(pd => pd.Subtotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PedidoDetalle>()
                .Property(pd => pd.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CategoriaMarca>()
                .HasKey(cm => cm.Id);

            modelBuilder.Entity<CategoriaMarca>()
                .HasOne(cm => cm.Categoria)
                .WithMany(c => c.CategoriasMarcas)
                .HasForeignKey(cm => cm.CategoriaId);

            modelBuilder.Entity<CategoriaMarca>()
                .HasOne(cm => cm.Marca)
                .WithMany(m => m.CategoriasMarcas)
                .HasForeignKey(cm => cm.MarcaId);
        }
    }
}
