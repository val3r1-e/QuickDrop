using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickDrop.Data;
using QuickDrop.Services;
using QuickDrop.Models;

namespace QuickDrop.Controllers;

public class AdminController : Controller
{
    private readonly QuickDropDbContext _db;
    public AdminController(QuickDropDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var user = AuthService.Current(_db, HttpContext);
        if (user == null || user.Role != "Admin") return RedirectToAction("Login", "Account");
        ViewBag.Users = await _db.Users.CountAsync();
        ViewBag.Restaurants = await _db.Restaurants.CountAsync();
        ViewBag.Products = await _db.Products.CountAsync();
        ViewBag.Orders = await _db.Orders.CountAsync();
        var totals = await _db.Orders.Select(o => o.Total).ToListAsync();
        ViewBag.Revenue = totals.Sum();
        ViewBag.ActiveDrivers = await _db.Users.CountAsync(u => u.Role == "Driver");
        ViewBag.OrdersToday = await _db.Orders.CountAsync(o => o.CreatedAt.Date == DateTime.UtcNow.Date);
        ViewBag.ActiveDriversList = await _db.Users
            .Where(u => u.Role == "Driver")
            .Select(u => new AdminActivityViewModel
            {
                Name = u.FullName,
                Status = "Available",
                Location = u.Phone
            })
            .ToListAsync();
        ViewBag.TopRestaurants = await _db.Restaurants
            .Select(r => new { r.Name, Orders = r.Products.Count })
            .Take(5)
            .ToListAsync();
        var orders = await _db.Orders.Include(o => o.Customer).Include(o => o.Restaurant).Include(o => o.Driver)
            .OrderByDescending(o => o.CreatedAt).Take(25).ToListAsync();
        return View(orders);
    }

    [HttpGet]
    public IActionResult Customers() => RedirectToAction(nameof(Index));

    [HttpGet]
    public IActionResult Drivers() => RedirectToAction(nameof(Index));

    [HttpGet]
    public IActionResult Orders() => RedirectToAction(nameof(Index));

    [HttpGet]
    public IActionResult Payments() => RedirectToAction(nameof(Index));

    [HttpGet]
    public IActionResult Reports() => RedirectToAction(nameof(Index));

    [HttpGet]
    public IActionResult Settings() => RedirectToAction(nameof(Index));
}
