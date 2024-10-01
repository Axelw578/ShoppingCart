namespace ShoppingCart.Models
{
    public class Carrito
    {
        public int CarritoId { get; set; }
        public int? PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        // Relación con CarritoItems
        public ICollection<CarritoItem> Items { get; set; }
    }
}

