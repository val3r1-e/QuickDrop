using System.ComponentModel.DataAnnotations;

namespace QuickDrop.Models;

public class RestaurantManagementViewModel
{
    public List<Restaurant> Restaurants { get; set; } = new();
    public CreateRestaurantViewModel NewRestaurant { get; set; } = new();
    public CreateProductViewModel NewProduct { get; set; } = new();
}

public class CreateRestaurantViewModel
{
    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Required, StringLength(100)]
    public string Cuisine { get; set; } = "";

    [Required, StringLength(250)]
    public string Address { get; set; } = "";

    [Range(-90, 90)]
    public double Latitude { get; set; } = 0.3476;

    [Range(-180, 180)]
    public double Longitude { get; set; } = 32.5825;

    [Required, Url, StringLength(2048)]
    public string ImageUrl { get; set; } = "";

    [Required, StringLength(100)]
    public string ProductName { get; set; } = "";

    [StringLength(500)]
    public string ProductDescription { get; set; } = "";

    [Required, StringLength(60)]
    public string ProductCategory { get; set; } = "";

    [Range(typeof(decimal), "1", "100000000")]
    public decimal ProductPrice { get; set; }

    [Required, Url, StringLength(2048)]
    public string ProductImageUrl { get; set; } = "";
}

public class CreateProductViewModel
{
    [Range(1, int.MaxValue)]
    public int RestaurantId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [StringLength(500)]
    public string Description { get; set; } = "";

    [Required, StringLength(60)]
    public string Category { get; set; } = "";

    [Range(typeof(decimal), "1", "100000000")]
    public decimal Price { get; set; }

    [Required, Url, StringLength(2048)]
    public string ImageUrl { get; set; } = "";
}
