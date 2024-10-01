namespace ShoppingCart.Models
{
    public class Pedido
    {
        public int PedidoId { get; set; }
        public DateTime FechaPedido { get; set; }
        public decimal Total { get; set; }

        // Relación con PedidoDetalles
        public virtual ICollection<PedidoDetalles> PedidoDetalles { get; set; } = new List<PedidoDetalles>();
    }
}
