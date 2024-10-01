using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;

namespace ShoppingCart.Models
{
    public class ShoppingCartContext : DbContext
    {
        public ShoppingCartContext(DbContextOptions<ShoppingCartContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<CarritoItem> CarritoItems { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalles> PedidoDetalles { get; set; } // Agregar esta línea

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PedidoDetalles>()
                .HasKey(pd => new { pd.DetalleId });

            modelBuilder.Entity<PedidoDetalles>()
                .HasOne(pd => pd.Pedido)
                .WithMany(p => p.PedidoDetalles)
                .HasForeignKey(pd => pd.PedidoId);

            modelBuilder.Entity<PedidoDetalles>()
                .HasOne(pd => pd.Producto)
                .WithMany()
                .HasForeignKey(pd => pd.ProductoId);
        }
    }
}
