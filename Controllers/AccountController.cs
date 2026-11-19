using Microsoft.AspNetCore.Mvc;
using QuickDrop.Data;
using QuickDrop.Services;

namespace QuickDrop.Controllers;

public class AccountController : Controller
{
    private readonly QuickDropDbContext _db;
    public AccountController(QuickDropDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var user = _db.Users.FirstOrDefault(x => x.Email == email && x.Password == password);
        if (user == null)
        {
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        AuthService.SignIn(HttpContext, user);
        return user.Role switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "Driver" => RedirectToAction("Index", "Driver"),
            "RestaurantManager" => RedirectToAction("Index", "Restaurant"),
            _ => RedirectToAction("Index", "Customer")
        };
    }

    public IActionResult Logout()
    {
        AuthService.SignOut(HttpContext);
        return RedirectToAction("Index", "Home");
    }
}
