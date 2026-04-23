using Microsoft.EntityFrameworkCore;
using Moq;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;
using ECommerceApi.Services;

namespace ECommerceApi.Tests;

public class CartServiceTests
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
        
        var userId = 1;
        var product = new Product { Id = 1, Name = "Laptop", Price = 1500, Stock = 10, ImageUrl = "laptop.jpg", CreatedAt = DateTime.UtcNow };
        context.Products.Add(product);
        
        var cartItem = new CartItem { Id = 1, UserId = userId, ProductId = 1, Quantity = 2 };
        context.CartItems.Add(cartItem);
        context.SaveChanges();
        
        return context;
    }

    [Fact]
    public async Task GetCartAsync_ReturnsCartItems_WhenExists()
    {
        using var context = CreateContextWithData();
        var service = new CartService(context);

        var result = await service.GetCartAsync(1);

        Assert.NotNull(result);
        var cartItems = result.ToList();
        Assert.Single(cartItems);
    }

    [Fact]
    public async Task GetCartAsync_ReturnsEmpty_WhenNoCart()
    {
        using var context = CreateContext();
        var service = new CartService(context);

        var result = await service.GetCartAsync(1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task AddToCartAsync_AddsNewItem_WhenNotInCart()
    {
        using var context = CreateContext();
        var product = new Product { Id = 1, Name = "Laptop", Price = 1500, Stock = 10, ImageUrl = "laptop.jpg", CreatedAt = DateTime.UtcNow };
        context.Products.Add(product);
        context.SaveChanges();
        
        var service = new CartService(context);
        var dto = new AddToCartDto { ProductId = 1, Quantity = 2 };

        var result = await service.AddToCartAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal(2, result.Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_UpdatesQuantity_WhenExists()
    {
        using var context = CreateContextWithData();
        var service = new CartService(context);
        var dto = new AddToCartDto { ProductId = 1, Quantity = 3 };

        var result = await service.AddToCartAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal(5, result.Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_Throws_WhenProductNotFound()
    {
        using var context = CreateContext();
        var service = new CartService(context);
        var dto = new AddToCartDto { ProductId = 999, Quantity = 1 };

        await Assert.ThrowsAsync<Exception>(() => service.AddToCartAsync(1, dto));
    }

    [Fact]
    public async Task UpdateQuantityAsync_UpdatesItem_WhenExists()
    {
        using var context = CreateContextWithData();
        var service = new CartService(context);

        var result = await service.UpdateQuantityAsync(1, 1, 5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Quantity);
    }

    [Fact]
    public async Task UpdateQuantityAsync_RemovesItem_WhenQuantityZero()
    {
        using var context = CreateContextWithData();
        var service = new CartService(context);

        var result = await service.UpdateQuantityAsync(1, 1, 0);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateQuantityAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContextWithData();
        var service = new CartService(context);

        var result = await service.UpdateQuantityAsync(1, 999, 5);

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveFromCartAsync_ReturnsTrue_WhenRemoved()
    {
        using var context = CreateContextWithData();
        var service = new CartService(context);

        var result = await service.RemoveFromCartAsync(1, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task RemoveFromCartAsync_ReturnsFalse_WhenNotExists()
    {
        using var context = CreateContextWithData();
        var service = new CartService(context);

        var result = await service.RemoveFromCartAsync(1, 999);

        Assert.False(result);
    }

    [Fact]
    public async Task ClearCartAsync_RemovesAllItems()
    {
        using var context = CreateContextWithData();
        var service = new CartService(context);

        await service.ClearCartAsync(1);

        var remaining = await service.GetCartAsync(1);
        Assert.Empty(remaining);
    }
}