using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;

public class PedidoDetalles
{
    public int DetalleId { get; set; }
    public int PedidoId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }

    // Navegación (opcional)
    public virtual Pedido Pedido { get; set; }
    public virtual Producto Producto { get; set; }
}
