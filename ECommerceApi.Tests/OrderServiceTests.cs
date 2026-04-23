using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;
using ECommerceApi.Services;

namespace ECommerceApi.Tests;

public class OrderServiceTests
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
        
        var user = new User { Id = 1, Email = "test@test.com", FullName = "Test User", CreatedAt = DateTime.UtcNow };
        var product = new Product { Id = 1, Name = "Laptop", Price = 1500, Stock = 10, ImageUrl = "laptop.jpg", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);
        context.Products.Add(product);
        
        var cartItem = new CartItem { Id = 1, UserId = 1, ProductId = 1, Quantity = 2 };
        context.CartItems.Add(cartItem);
        context.SaveChanges();
        
        return context;
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsOrders_WhenExists()
    {
        using var context = CreateContextWithData();
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
        context.SaveChanges();
        
        var service = new OrderService(context);

        var result = await service.GetOrdersAsync(1);

        Assert.NotNull(result);
        var orders = result.ToList();
        Assert.Single(orders);
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsEmpty_WhenNoOrders()
    {
        using var context = CreateContext();
        var service = new OrderService(context);

        var result = await service.GetOrdersAsync(1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsOrder_WhenExists()
    {
        using var context = CreateContextWithData();
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
        context.SaveChanges();
        
        var service = new OrderService(context);

        var result = await service.GetOrderByIdAsync(1, 1);

        Assert.NotNull(result);
        Assert.Equal(3000, result.TotalAmount);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContextWithData();
        var service = new OrderService(context);

        var result = await service.GetOrderByIdAsync(1, 999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateOrderAsync_CreatesOrder_FromCart()
    {
        using var context = CreateContextWithData();
        var service = new OrderService(context);
        
        var dto = new CreateOrderDto { ShippingAddress = "123 Main St" };

        var result = await service.CreateOrderAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal(3000, result.TotalAmount);
    }

    [Fact]
    public async Task CreateOrderAsync_Throws_WhenCartEmpty()
    {
        using var context = CreateContext();
        var user = new User { Id = 1, Email = "test@test.com", FullName = "Test User", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);
        context.SaveChanges();
        
        var service = new OrderService(context);
        var dto = new CreateOrderDto { ShippingAddress = "123 Main St" };

        await Assert.ThrowsAsync<Exception>(() => service.CreateOrderAsync(1, dto));
    }

    [Fact]
    public async Task CreateOrderAsync_ReducesStock_AfterOrder()
    {
        using var context = CreateContextWithData();
        var service = new OrderService(context);
        
        var dto = new CreateOrderDto { ShippingAddress = "123 Main St" };
        
        var productBefore = await context.Products.FindAsync(1);
        var initialStock = productBefore!.Stock;
        
        await service.CreateOrderAsync(1, dto);
        
        var productAfter = await context.Products.FindAsync(1);
        Assert.Equal(initialStock - 2, productAfter!.Stock);
    }
}