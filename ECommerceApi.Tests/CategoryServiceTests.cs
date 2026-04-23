using Microsoft.EntityFrameworkCore;
using Moq;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;
using ECommerceApi.Services;

namespace ECommerceApi.Tests;

public class CategoryServiceTests
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
        
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", Description = "Electronic devices" },
            new() { Id = 2, Name = "Clothing", Description = "Fashion items" }
        };
        context.Categories.AddRange(categories);
        
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 1500, Stock = 10, CategoryId = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Mouse", Price = 25, Stock = 100, CategoryId = 1, CreatedAt = DateTime.UtcNow }
        };
        context.Products.AddRange(products);
        context.SaveChanges();
        
        return context;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCategories_WhenDataExists()
    {
        using var context = CreateContextWithData();
        var service = new CategoryService(context);

        var result = await service.GetAllAsync();

        Assert.NotNull(result);
        var categoryList = result.ToList();
        Assert.Equal(2, categoryList.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoData()
    {
        using var context = CreateContext();
        var service = new CategoryService(context);

        var result = await service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategory_WhenExists()
    {
        using var context = CreateContextWithData();
        var service = new CategoryService(context);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContextWithData();
        var service = new CategoryService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedCategory()
    {
        using var context = CreateContext();
        var service = new CategoryService(context);
        
        var dto = new CreateCategoryDto
        {
            Name = "Books",
            Description = "All kinds of books"
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Books", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        using var context = CreateContextWithData();
        var service = new CategoryService(context);

        var result = await service.DeleteAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        using var context = CreateContextWithData();
        var service = new CategoryService(context);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }
}