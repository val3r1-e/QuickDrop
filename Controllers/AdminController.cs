using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickDrop.Data;
using QuickDrop.Services;

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
        ViewBag.Revenue = await _db.Orders.SumAsync(o => (decimal?)o.Total) ?? 0;
        var orders = await _db.Orders.Include(o => o.Customer).Include(o => o.Restaurant).Include(o => o.Driver)
            .OrderByDescending(o => o.CreatedAt).Take(25).ToListAsync();
        return View(orders);
    }
}
