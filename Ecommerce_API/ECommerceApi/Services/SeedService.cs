using ECommerceApi.Data;
using ECommerceApi.Entities;

namespace ECommerceApi.Services;

public class SeedService
{
    private readonly AppDbContext _context;
    
    public SeedService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task SeedDataAsync()
    {
        if (_context.Products.Any()) return;
        
        var electronics = new Category { Name = "Electronics", Description = "Electronic devices and gadgets" };
        var clothing = new Category { Name = "Clothing", Description = "Fashion and apparel" };
        var books = new Category { Name = "Books", Description = "Books and literature" };
        
        _context.Categories.AddRange(electronics, clothing, books);
        await _context.SaveChangesAsync();
        
        var products = new List<Product>
        {
            new Product
            {
                Name = "Laptop Pro 15",
                Description = "High performance laptop with 16GB RAM",
                Price = 25000m,
                Stock = 50,
                ImageUrl = "https://via.placeholder.com/150",
                CategoryId = electronics.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse",
                Price = 450m,
                Stock = 200,
                ImageUrl = "https://via.placeholder.com/150",
                CategoryId = electronics.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "USB-C Hub",
                Description = "7-in-1 USB-C hub adapter",
                Price = 800m,
                Stock = 150,
                ImageUrl = "https://via.placeholder.com/150",
                CategoryId = electronics.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Cotton T-Shirt",
                Description = "Comfortable cotton t-shirt",
                Price = 250m,
                Stock = 500,
                ImageUrl = "https://via.placeholder.com/150",
                CategoryId = clothing.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Denim Jeans",
                Description = "Classic denim jeans",
                Price = 800m,
                Stock = 100,
                ImageUrl = "https://via.placeholder.com/150",
                CategoryId = clothing.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "C# Programming",
                Description = "Learn C# programming basics",
                Price = 350m,
                Stock = 75,
                ImageUrl = "https://via.placeholder.com/150",
                CategoryId = books.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "ASP.NET Core Guide",
                Description = "Master ASP.NET Core web development",
                Price = 500m,
                Stock = 50,
                ImageUrl = "https://via.placeholder.com/150",
                CategoryId = books.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Wireless Headphones",
                Description = "Noise cancelling headphones",
                Price = 1500m,
                Stock = 80,
                ImageUrl = "https://via.placeholder.com/150",
                CategoryId = electronics.Id,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();
        
        Console.WriteLine("Seed data added successfully!");
    }
}