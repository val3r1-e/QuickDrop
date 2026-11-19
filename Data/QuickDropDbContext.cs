using Microsoft.EntityFrameworkCore;
using QuickDrop.Models;

namespace QuickDrop.Data;

public class QuickDropDbContext : DbContext
{
    public QuickDropDbContext(DbContextOptions<QuickDropDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}
