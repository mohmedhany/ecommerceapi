using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;
using ECommerceApi.Services;

namespace ECommerceApi.Tests;

public class DashboardServiceTests
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
        
        var users = new List<User>
        {
            new() { Id = 1, Email = "user1@test.com", FullName = "User 1", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Email = "user2@test.com", FullName = "User 2", CreatedAt = DateTime.UtcNow.AddDays(-60) }
        };
        context.Users.AddRange(users);
        
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 1500, Stock = 5, CategoryId = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Mouse", Price = 25, Stock = 100, CategoryId = 1, CreatedAt = DateTime.UtcNow }
        };
        context.Products.AddRange(products);
        
        var order = new Order
        {
            Id = 1,
            UserId = 1,
            ShippingAddress = "123 Main St",
            TotalAmount = 3000,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        
        var orderItem = new OrderItem
        {
            Id = 1,
            OrderId = 1,
            ProductId = 1,
            Quantity = 2,
            UnitPrice = 1500
        };
        context.OrderItems.Add(orderItem);
        context.SaveChanges();
        
        return context;
    }

    [Fact]
    public async Task GetDashboardStats_ReturnsCorrectStats()
    {
        using var context = CreateContextWithData();
        var service = new DashboardService(context);

        var result = await service.GetDashboardStats();

        var stats = result.GetType().GetProperty("totalProducts");
        Assert.Equal(2, stats!.GetValue(result));
    }

    [Fact]
    public async Task GetRevenueReport_ReturnsData_WhenOrdersExist()
    {
        using var context = CreateContextWithData();
        var service = new DashboardService(context);

        var result = await service.GetRevenueReport(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTopProducts_ReturnsProducts_WhenOrdersExist()
    {
        using var context = CreateContextWithData();
        var service = new DashboardService(context);

        var result = await service.GetTopProducts(10);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetLowStockProducts_ReturnsLowStock_WhenThresholdMet()
    {
        using var context = CreateContextWithData();
        var service = new DashboardService(context);

        var result = await service.GetLowStockProducts(10);

        Assert.NotNull(result);
        var first = result.FirstOrDefault();
        Assert.NotNull(first);
    }

    [Fact]
    public async Task GetOrdersStats_ReturnsStatusCounts()
    {
        using var context = CreateContextWithData();
        var service = new DashboardService(context);

        var result = await service.GetOrdersStats();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUserStats_ReturnsCorrectCounts()
    {
        using var context = CreateContextWithData();
        var service = new DashboardService(context);

        var result = await service.GetUserStats();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
}