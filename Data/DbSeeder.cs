using QuickDrop.Models;

namespace QuickDrop.Data;

public static class DbSeeder
{
    public static void Seed(QuickDropDbContext db)
    {
        if (db.Users.Any()) return;

        db.Users.AddRange(
            new AppUser { FullName = "Admin", Email = "admin@gmail.com", Password = "Admin123!", Role = "Admin", Phone = "0700000000" },
            new AppUser { FullName = "Restaurant Manager", Email = "manager@gmail.com", Password = "Manager123!", Role = "RestaurantManager", Phone = "0700000001" },
            new AppUser { FullName = "Delivery Driver", Email = "driver@gmail.com", Password = "Driver123!", Role = "Driver", Phone = "0700000002" },
            new AppUser { FullName = "Demo Customer", Email = "customer@gmail.com", Password = "Customer123!", Role = "Customer", Phone = "0700000003" }
        );

        var restaurants = new[]
        {
            new Restaurant { Name = "KFC Uganda", Cuisine = "Chicken & Fast Food", Address = "Acacia Mall, Kampala", Latitude = 0.3503, Longitude = 32.5886, ImageUrl = "https://images.unsplash.com/photo-1513639776629-7b61b0ac49cb?auto=format&fit=crop&w=900&q=80" },
            new Restaurant { Name = "Cafe Javas", Cuisine = "African • Cafe • International", Address = "Bombo Road, Kampala", Latitude = 0.3476, Longitude = 32.5825, ImageUrl = "https://images.unsplash.com/photo-1552566626-52f8b828add9?auto=format&fit=crop&w=900&q=80" },
            new Restaurant { Name = "Nando's", Cuisine = "Peri-Peri Chicken", Address = "Garden City, Kampala", Latitude = 0.3207, Longitude = 32.5850, ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=900&q=80" },
            new Restaurant { Name = "Pizza Hut", Cuisine = "Pizza & Italian", Address = "Kampala Road, Kampala", Latitude = 0.3163, Longitude = 32.5810, ImageUrl = "https://images.unsplash.com/photo-1579751626657-72bc17010498?auto=format&fit=crop&w=900&q=80" }
        };
        db.Restaurants.AddRange(restaurants);
        db.SaveChanges();

        var products = new List<Product>
        {
            new Product { RestaurantId = restaurants[0].Id, Name = "Zinger Burger", Description = "Crispy chicken fillet, lettuce and sauce.", Price = 14000, Category = "Burgers", ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80" },
            new Product { RestaurantId = restaurants[0].Id, Name = "Chicken Bucket", Description = "Crispy chicken pieces for sharing.", Price = 32000, Category = "Chicken", ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?auto=format&fit=crop&w=800&q=80" },
            new Product { RestaurantId = restaurants[1].Id, Name = "Chicken & Chips", Description = "Grilled chicken served with crispy fries.", Price = 18000, Category = "Meals", ImageUrl = "https://images.unsplash.com/photo-1562967914-608f82629710?auto=format&fit=crop&w=800&q=80" },
            new Product { RestaurantId = restaurants[1].Id, Name = "Iced Coffee", Description = "Cold coffee with milk and ice.", Price = 9000, Category = "Drinks", ImageUrl = "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=80" },
            new Product { RestaurantId = restaurants[2].Id, Name = "Peri-Peri Chicken", Description = "Flame-grilled chicken with peri-peri sauce.", Price = 25000, Category = "Chicken", ImageUrl = "https://images.unsplash.com/photo-1598103442097-8b74394b95c6?auto=format&fit=crop&w=800&q=80" },
            new Product { RestaurantId = restaurants[3].Id, Name = "Pepperoni Pizza", Description = "Classic pizza topped with pepperoni and mozzarella.", Price = 30000, Category = "Pizza", ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?auto=format&fit=crop&w=800&q=80" }
        };
        db.Products.AddRange(products);
        db.SaveChanges();
    }
}
