using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;

public class CarritoService
{
    private readonly ShoppingCartContext _context;

    public CarritoService(ShoppingCartContext context)
    {
        _context = context;
    }

    // Método para obtener la cantidad total de artículos en el carrito
    public int ObtenerCantidadEnCarrito(int carritoId)
    {
        return _context.CarritoItems
            .Where(ci => ci.CarritoId == carritoId)
            .Sum(ci => ci.Cantidad);
    }

    // Método para obtener el carrito con los items y productos relacionados
    public Carrito ObtenerCarritoConItems(int carritoId)
    {
        return _context.Carritos
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Producto)
            .FirstOrDefault(c => c.CarritoId == carritoId);
    }

    // Método para agregar productos al carrito
    public void AgregarAlCarrito(int productoId)
    {
        var producto = _context.Productos.Find(productoId);
        if (producto == null) return;

        var carrito = _context.Carritos.FirstOrDefault();
        if (carrito == null)
        {
            carrito = new Carrito();
            _context.Carritos.Add(carrito);
            _context.SaveChanges();
        }

        var carritoItem = _context.CarritoItems
            .FirstOrDefault(ci => ci.CarritoId == carrito.CarritoId && ci.ProductoId == productoId);

        if (carritoItem != null)
        {
            carritoItem.Cantidad++;
        }
        else
        {
            carritoItem = new CarritoItem
            {
                ProductoId = productoId,
                Cantidad = 1,
                PrecioUnitario = producto.Precio,
                CarritoId = carrito.CarritoId
            };
            _context.CarritoItems.Add(carritoItem);
        }

        _context.SaveChanges();
    }

    public int ObtenerCantidadProductos()
    {
        return _context.CarritoItems.Sum(ci => ci.Cantidad);
    }

}
