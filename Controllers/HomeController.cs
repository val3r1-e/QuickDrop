using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickDrop.Data;

namespace QuickDrop.Controllers;

public class HomeController : Controller
{
    private readonly QuickDropDbContext _db;
    public HomeController(QuickDropDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        q = q?.Trim();
        var restaurants = await _db.Restaurants
            .Include(r => r.Products)
            .Where(r => string.IsNullOrWhiteSpace(q)
                || r.Name.Contains(q)
                || r.Cuisine.Contains(q)
                || r.Address.Contains(q)
                || r.Products.Any(p => p.Name.Contains(q) || p.Category.Contains(q)))
            .ToListAsync();
        ViewBag.Query = q;
        return View(restaurants);
    }

    public IActionResult Error() => View();
}
