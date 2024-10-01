using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class CarritoController : Controller
{
    private readonly CarritoService _carritoService;
    private readonly ShoppingCartContext _context;

    public CarritoController(CarritoService carritoService, ShoppingCartContext context)
    {
        _carritoService = carritoService;
        _context = context;
    }

    private int ObtenerCarritoId()
    {

        return 1; 
    }

    public IActionResult MostrarCarrito()
    {
        var carritoId = ObtenerCarritoId();
        var carritoItems = _context.CarritoItems
            .Where(ci => ci.CarritoId == carritoId)
            .Include(ci => ci.Producto) 
            .ToList();

        var totalSeleccionado = carritoItems
            .Where(ci => ci.Cantidad > 0)
            .Sum(ci => ci.PrecioUnitario * ci.Cantidad);

        var model = new Carrito
        {
            Items = carritoItems
        };

        // Contador de productos en el carrito
        ViewBag.CarritoCount = carritoItems.Sum(ci => ci.Cantidad);
        ViewBag.TotalSeleccionado = totalSeleccionado > 0 ? totalSeleccionado : 0.00m;

        return View(model);
    }

    [HttpPost]
    public IActionResult EliminarDelCarrito(int productoId)
    {
        var carritoId = ObtenerCarritoId();

        var carritoItem = _context.CarritoItems
            .FirstOrDefault(ci => ci.ProductoId == productoId && ci.CarritoId == carritoId);

        if (carritoItem != null)
        {
            _context.CarritoItems.Remove(carritoItem);
            _context.SaveChanges();
        }

        var carritoCount = _context.CarritoItems.Sum(ci => ci.Cantidad);

        // Devolver un JSON con el recuento actualizado del carrito y un mensaje
        return Json(new
        {
            carritoCount,
            mensaje = carritoCount > 0 ? "Producto eliminado del carrito." : "El carrito está vacío."
        });
    }

    [HttpPost]
    public IActionResult AgregarAlCarrito(int productoId)
    {
        var producto = _context.Productos.Find(productoId);

        if (producto != null)
        {
            var carritoId = ObtenerCarritoId();
            var carrito = _context.Carritos.FirstOrDefault(c => c.CarritoId == carritoId);


            if (carrito == null)
            {
                carrito = new Carrito();
                _context.Carritos.Add(carrito);
                _context.SaveChanges();
            }

            var carritoItem = _context.CarritoItems
                .FirstOrDefault(ci => ci.CarritoId == carrito.CarritoId && ci.ProductoId == producto.ProductoId);

            if (carritoItem != null)
            {
                carritoItem.Cantidad++;
            }
            else
            {
                carritoItem = new CarritoItem
                {
                    ProductoId = producto.ProductoId,
                    Cantidad = 1,
                    PrecioUnitario = producto.Precio,
                    CarritoId = carrito.CarritoId
                };
                _context.CarritoItems.Add(carritoItem);
            }

            _context.SaveChanges();

            // Devuelve el nuevo número de productos en el carrito y un mensaje de éxito en formato JSON
            return Json(new
            {
                carritoCount = _context.CarritoItems.Sum(ci => ci.Cantidad),
                mensaje = $"{producto.Nombre} ha sido agregado al carrito."
            });
        }

        return Json(new { carritoCount = 0, mensaje = "Producto no encontrado." });


    }


    [HttpPost]
    public IActionResult FinalizarPedido()
    {
        var carritoId = ObtenerCarritoId();
        var carritoItems = _context.CarritoItems.Where(ci => ci.CarritoId == carritoId).ToList();

        if (carritoItems.Count == 0)
        {
            TempData["Mensaje"] = "No hay productos en el carrito para finalizar el pedido.";
            return RedirectToAction("MostrarCarrito");
        }

        // Lógica para crear el pedido
        var pedido = new Pedido
        {
            FechaPedido = DateTime.Now,
            Total = carritoItems.Sum(ci => ci.Cantidad * ci.PrecioUnitario) // Calcular total
        };
        _context.Pedidos.Add(pedido);
        _context.SaveChanges(); // Guardamos para obtener el ID del pedido

        // Lógica para agregar detalles del pedido
        foreach (var carritoItem in carritoItems)
        {
            var pedidoDetalle = new PedidoDetalles
            {
                PedidoId = pedido.PedidoId, // Relacionar con el nuevo pedido
                ProductoId = carritoItem.ProductoId,
                Cantidad = carritoItem.Cantidad
            };
            _context.PedidoDetalles.Add(pedidoDetalle);
        }

        _context.CarritoItems.RemoveRange(carritoItems); // Limpiar el carrito después de finalizar el pedido
        _context.SaveChanges();

        TempData["Mensaje"] = "Tu pedido ha sido finalizado con éxito.";
        return RedirectToAction("Index", "Productos");
    }



    [HttpPost]
    public IActionResult ActualizarCantidad(int productoId, int cantidad)
    {
        var carritoId = ObtenerCarritoId();

        var carritoItem = _context.CarritoItems
            .FirstOrDefault(ci => ci.ProductoId == productoId && ci.CarritoId == carritoId);

        if (carritoItem != null && cantidad > 0)
        {
            carritoItem.Cantidad = cantidad;
            _context.SaveChanges();

            var carritoItems = _context.CarritoItems.Where(ci => ci.CarritoId == carritoId).ToList();
            var subtotal = carritoItem.Cantidad * carritoItem.PrecioUnitario;
            var total = carritoItems.Sum(ci => ci.Cantidad * ci.PrecioUnitario);

            // Devuelve el subtotal y el total actualizados
            return Json(new
            {
                subtotal = subtotal,
                total = total,
                carritoCount = carritoItems.Sum(ci => ci.Cantidad)
            });
        }

        return Json(new { subtotal = 0, total = 0, carritoCount = 0 });
    }


    [HttpPost]
    public IActionResult ComprarSeleccionados(int[] productosSeleccionados)
    {
        decimal total = 0;

        if (productosSeleccionados != null && productosSeleccionados.Length > 0)
        {
            var carritoId = ObtenerCarritoId();

            // Crear un nuevo pedido
            var pedido = new Pedido
            {
                FechaPedido = DateTime.Now,
                Total = 0 // Esto se actualizará más adelante
            };
            _context.Pedidos.Add(pedido);
            _context.SaveChanges(); // Guardamos para obtener el ID del pedido

            foreach (var productoId in productosSeleccionados)
            {
                var carritoItem = _context.CarritoItems
                    .FirstOrDefault(ci => ci.CarritoId == carritoId && ci.ProductoId == productoId);

                if (carritoItem != null)
                {
                    // Calcular el total para cada producto seleccionado
                    total += carritoItem.Cantidad * carritoItem.PrecioUnitario;

                    // Crear detalle del pedido
                    var pedidoDetalle = new PedidoDetalles
                    {
                        PedidoId = pedido.PedidoId, // Relacionar con el nuevo pedido
                        ProductoId = carritoItem.ProductoId,
                        Cantidad = carritoItem.Cantidad
                    };
                    _context.PedidoDetalles.Add(pedidoDetalle);

                    // Eliminar el producto del carrito después de la compra
                    _context.CarritoItems.Remove(carritoItem);
                }
            }

            // Actualizar el total del pedido
            pedido.Total = total;
            _context.SaveChanges();

            TempData["Mensaje"] = "Has comprado los productos seleccionados con éxito.";
        }
        else
        {
            TempData["Mensaje"] = "No has seleccionado ningún producto para comprar.";
        }

        return RedirectToAction("MostrarCarrito");
    }


    [HttpGet]
    public IActionResult ObtenerContadorCarrito()
    {
        var carritoId = ObtenerCarritoId();
        var totalProductos = _context.CarritoItems
            .Where(ci => ci.CarritoId == carritoId)
            .Sum(ci => ci.Cantidad);

        return Json(totalProductos);
    }

    [HttpGet]
    public IActionResult ObtenerCarritoCount()
    {
        var carritoId = ObtenerCarritoId(); // Método que obtiene el ID del carrito actual
        var carritoCount = _context.CarritoItems
                            .Where(ci => ci.CarritoId == carritoId)
                            .Sum(ci => ci.Cantidad);

        return Json(new { carritoCount = carritoCount });
    }




}
