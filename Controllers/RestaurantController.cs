using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickDrop.Data;
using QuickDrop.Models;
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

    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        if (!IsRestaurantManager()) return RedirectToAction("Login", "Account");

        var model = new RestaurantManagementViewModel
        {
            Restaurants = await _db.Restaurants
                .Include(r => r.Products)
                .OrderBy(r => r.Name)
                .ToListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRestaurant(CreateRestaurantViewModel input)
    {
        if (!IsRestaurantManager()) return Unauthorized();
        if (!ModelState.IsValid) return await ManageWithInput(input);

        var restaurant = new Restaurant
        {
            Name = input.Name.Trim(),
            Cuisine = input.Cuisine.Trim(),
            Address = input.Address.Trim(),
            Latitude = input.Latitude,
            Longitude = input.Longitude,
            ImageUrl = input.ImageUrl.Trim(),
            Products =
            {
                new Product
                {
                    Name = input.ProductName.Trim(),
                    Description = input.ProductDescription.Trim(),
                    Category = input.ProductCategory.Trim(),
                    Price = input.ProductPrice,
                    ImageUrl = input.ProductImageUrl.Trim()
                }
            }
        };

        _db.Restaurants.Add(restaurant);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"{restaurant.Name} and its first menu item were added.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(CreateProductViewModel input)
    {
        if (!IsRestaurantManager()) return Unauthorized();
        if (!ModelState.IsValid) return await ManageWithInput(productInput: input);
        if (!await _db.Restaurants.AnyAsync(r => r.Id == input.RestaurantId)) return NotFound();

        _db.Products.Add(new Product
        {
            RestaurantId = input.RestaurantId,
            Name = input.Name.Trim(),
            Description = input.Description.Trim(),
            Category = input.Category.Trim(),
            Price = input.Price,
            ImageUrl = input.ImageUrl.Trim()
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"{input.Name} was added to the menu.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var user = AuthService.Current(_db, HttpContext);
        if (user == null || user.Role != "RestaurantManager") return Unauthorized();
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        if (status != "Accepted" && status != "Preparing" && status != "Ready for Pickup") return BadRequest("Invalid restaurant status.");
        order.Status = status;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    private bool IsRestaurantManager()
    {
        var user = AuthService.Current(_db, HttpContext);
        return user?.Role == "RestaurantManager";
    }

    private async Task<IActionResult> ManageWithInput(
        CreateRestaurantViewModel? restaurantInput = null,
        CreateProductViewModel? productInput = null)
    {
        var model = new RestaurantManagementViewModel
        {
            Restaurants = await _db.Restaurants
                .Include(r => r.Products)
                .OrderBy(r => r.Name)
                .ToListAsync(),
            NewRestaurant = restaurantInput ?? new CreateRestaurantViewModel(),
            NewProduct = productInput ?? new CreateProductViewModel()
        };
        return View("Manage", model);
    }
}
