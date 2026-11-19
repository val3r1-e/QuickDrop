using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickDrop.Data;
using QuickDrop.Services;

namespace QuickDrop.Controllers;

public class DriverController : Controller
{
    private readonly QuickDropDbContext _db;
    public DriverController(QuickDropDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var user = AuthService.Current(_db, HttpContext);
        if (user == null || user.Role != "Driver") return RedirectToAction("Login", "Account");
        var orders = await _db.Orders.Include(o => o.Restaurant).Include(o => o.Customer)
            .Where(o => o.DriverId == user.Id || o.DriverId == null)
            .OrderByDescending(o => o.CreatedAt).Take(20).ToListAsync();
        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Accept(int id)
    {
        var user = AuthService.Current(_db, HttpContext);
        if (user == null || user.Role != "Driver") return Unauthorized();
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.DriverId = user.Id;
        order.Status = "Picked Up";
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var user = AuthService.Current(_db, HttpContext);
        if (user == null || user.Role != "Driver") return Unauthorized();
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = status;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
