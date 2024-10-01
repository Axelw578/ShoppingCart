using ShoppingCart.Models;

namespace ShoppingCart.Models
{
    public class CarritoItem
    {
        public int CarritoItemId { get; set; }
        public int CarritoId { get; set; }
        public Carrito Carrito { get; set; } // Asegúrate de que esta referencia sea válida
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
