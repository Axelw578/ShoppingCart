namespace ShoppingCart.Models
{
    public class Producto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Imagen { get; set; } // Añade esta línea

        public ICollection<CarritoItem> CarritoItems { get; set; }
    }
}
