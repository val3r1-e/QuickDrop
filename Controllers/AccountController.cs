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
    [ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password)
    {
        email = email?.Trim() ?? "";
        password = password ?? "";
        var user = _db.Users.FirstOrDefault(x => x.Email.ToLower() == email.ToLower() && x.Password == password);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        AuthService.SignOut(HttpContext);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.Error = "Registration is not enabled in this demo.";
        return View("Login");
    }

    [HttpGet]
    public IActionResult Forgot()
    {
        ViewBag.Error = "Password recovery is not enabled in this demo.";
        return View("Login");
    }
}
