using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using Microsoft.EntityFrameworkCore;

public class ProductosController : Controller
{
    private readonly ShoppingCartContext _context;
    private readonly CarritoService _carritoService; // Añadido

    public ProductosController(ShoppingCartContext context, CarritoService carritoService) // Añadido
    {
        _context = context;
        _carritoService = carritoService; // Añadido
    }

    public IActionResult Index()
    {
        var productos = _context.Productos.ToList();
        return View(productos);
    }
    [HttpPost]
    public IActionResult AgregarAlCarrito(int productoId)
    {
        // Llama al método del servicio para agregar el producto al carrito
        _carritoService.AgregarAlCarrito(productoId);



        // Mensaje al agregar producto
        TempData["Mensaje"] = "El producto ha sido agregado al carrito.";


        // Devuelve una respuesta en formato JSON, asegurando que la solicitud AJAX lo reciba
        return Json(new { success = true });
    }




}
