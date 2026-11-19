using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickDrop.Data;
using QuickDrop.Services;

namespace QuickDrop.Controllers;

public class RestaurantController : Controller
{
    private readonly QuickDropDbContext _db;
    public RestaurantController(QuickDropDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var user = AuthService.Current(_db, HttpContext);
        if (user == null || user.Role != "RestaurantManager") return RedirectToAction("Login", "Account");
        var orders = await _db.Orders.Include(o => o.Restaurant).Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt).Take(30).ToListAsync();
        var restaurants = await _db.Restaurants.Include(r => r.Products).ToListAsync();
        ViewBag.Restaurants = restaurants;
        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var user = AuthService.Current(_db, HttpContext);
        if (user == null || user.Role != "RestaurantManager") return Unauthorized();
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = status;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
