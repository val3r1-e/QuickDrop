using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickDrop.Data;
using QuickDrop.Models;
using QuickDrop.Services;

namespace QuickDrop.Controllers;

public class CustomerController : Controller
{
    private readonly QuickDropDbContext _db;
    public CustomerController(QuickDropDbContext db) => _db = db;

    private AppUser? UserNow => AuthService.Current(_db, HttpContext);

    public async Task<IActionResult> Index()
    {
        var user = UserNow;
        if (user == null) return RedirectToAction("Login", "Account");
        var orders = await _db.Orders.Include(o => o.Restaurant).Where(o => o.CustomerId == user.Id).OrderByDescending(o => o.CreatedAt).Take(10).ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Restaurant(int id)
    {
        var restaurant = await _db.Restaurants.Include(r => r.Products).FirstOrDefaultAsync(r => r.Id == id);
        return restaurant == null ? NotFound() : View(restaurant);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(int productId, int quantity, string deliveryAddress, string paymentMethod, double deliveryLat = 0.3476, double deliveryLng = 32.5825)
    {
        var user = UserNow;
        if (user == null) return RedirectToAction("Login", "Account");

        var product = await _db.Products.Include(p => p.Restaurant).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return NotFound();

        var order = new Order
        {
            CustomerId = user.Id,
            RestaurantId = product.RestaurantId,
            Total = product.Price * quantity + 3000,
            DeliveryAddress = deliveryAddress,
            DeliveryLat = deliveryLat,
            DeliveryLng = deliveryLng,
            PaymentMethod = paymentMethod,
            PaymentStatus = paymentMethod == "Cash" ? "Pending" : "Demo Paid",
            Status = "Pending"
        };
        order.Items.Add(new OrderItem { ProductId = product.Id, Quantity = quantity, UnitPrice = product.Price });
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Order #{order.Id} placed successfully. Payment: {order.PaymentStatus}.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Track(int id)
    {
        var order = await _db.Orders.Include(o => o.Restaurant).Include(o => o.Driver).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        ViewBag.GoogleMapsKey = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["GoogleMaps:ApiKey"];
        return View(order);
    }
}
