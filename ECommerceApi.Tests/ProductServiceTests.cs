using Microsoft.EntityFrameworkCore;
using Moq;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;
using ECommerceApi.Services;

namespace ECommerceApi.Tests;

public class ProductServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private AppDbContext CreateContextWithData()
    {
        var context = CreateContext();
        
        var category = new Category { Id = 1, Name = "Electronics" };
        context.Categories.Add(category);
        
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Description = "Gaming Laptop", Price = 1500, Stock = 10, CategoryId = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Mouse", Description = "Wireless Mouse", Price = 25, Stock = 100, CategoryId = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = 3, Name = "Keyboard", Description = "Mechanical Keyboard", Price = 80, Stock = 50, CategoryId = 1, CreatedAt = DateTime.UtcNow }
        };
        context.Products.AddRange(products);
        context.SaveChanges();
        
        return context;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProducts_WhenDataExists()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);

        var result = await service.GetAllAsync(1, 10);

        Assert.NotNull(result);
        var productList = result.ToList();
        Assert.Equal(3, productList.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoData()
    {
        using var context = CreateContext();
        var service = new ProductService(context);

        var result = await service.GetAllAsync(1, 10);

        var productList = result.ToList();
        Assert.Empty(productList);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Laptop", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingProducts()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);

        var result = await service.SearchAsync("Laptop");

        var productList = result.ToList();
        Assert.Single(productList);
        Assert.Equal("Laptop", productList[0].Name);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenNoMatch()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);

        var result = await service.SearchAsync("NonExistent");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsProductsByCategory()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);

        var result = await service.GetByCategoryAsync(1);

        var productList = result.ToList();
        Assert.Equal(3, productList.Count);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedProduct()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);
        
        var dto = new CreateProductDto
        {
            Name = "Monitor",
            Description = "4K Monitor",
            Price = 400,
            Stock = 20,
            CategoryId = 1
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Monitor", result.Name);
        Assert.Equal(400, result.Price);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProduct_WhenExists()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);
        
        var dto = new UpdateProductDto { Price = 2000 };

        var result = await service.UpdateAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal(2000, result.Price);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);
        
        var dto = new UpdateProductDto { Price = 2000 };

        var result = await service.UpdateAsync(999, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);

        var result = await service.DeleteAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        using var context = CreateContextWithData();
        var service = new ProductService(context);

        var result = await service.GetTotalCountAsync();

        Assert.Equal(3, result);
    }
}