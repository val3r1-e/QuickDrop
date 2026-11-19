namespace QuickDrop.Models;

public class AppUser
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = ""; // Demo only. Use ASP.NET Identity/password hashing in production.
    public string Role { get; set; } = "Customer";
    public string Phone { get; set; } = "";
}

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Cuisine { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string Address { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsOpen { get; set; } = true;
    public List<Product> Products { get; set; } = new();
}

public class Product
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = "";
    public string Category { get; set; } = "";
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public AppUser? Customer { get; set; }
    public int? DriverId { get; set; }
    public AppUser? Driver { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Pending";
    public string PaymentMethod { get; set; } = "Cash, mobile money" ;
    public decimal Total { get; set; }
    public string DeliveryAddress { get; set; } = "";
    public double DeliveryLat { get; set; }
    public double DeliveryLng { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class AdminActivityViewModel
{
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
    public string Location { get; set; } = "";
}
