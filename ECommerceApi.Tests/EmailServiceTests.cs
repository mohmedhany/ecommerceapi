using Microsoft.Extensions.Configuration;
using ECommerceApi.Entities;
using ECommerceApi.Services;

namespace ECommerceApi.Tests;

public class EmailServiceTests
{
    private IConfiguration CreateConfiguration()
    {
        var dict = new Dictionary<string, string?>
        {
            { "AppUrl", "http://localhost:5000" }
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_DoesNotThrow()
    {
        var config = CreateConfiguration();
        var service = new EmailService(config);
        
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FullName = "Test User",
            CreatedAt = DateTime.UtcNow
        };

        await service.SendWelcomeEmailAsync(user);
    }

    [Fact]
    public async Task SendOrderConfirmationAsync_DoesNotThrow()
    {
        var config = CreateConfiguration();
        var service = new EmailService(config);
        
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FullName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        
        var order = new Order
        {
            Id = 1,
            UserId = 1,
            ShippingAddress = "123 Main St",
            TotalAmount = 3000,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await service.SendOrderConfirmationAsync(order, user);
    }

    [Fact]
    public async Task SendOrderStatusUpdateAsync_DoesNotThrow()
    {
        var config = CreateConfiguration();
        var service = new EmailService(config);
        
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FullName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        
        var order = new Order
        {
            Id = 1,
            UserId = 1,
            Status = OrderStatus.Shipped,
            CreatedAt = DateTime.UtcNow
        };

        await service.SendOrderStatusUpdateAsync(order, user);
    }

    [Fact]
    public async Task SendPasswordResetAsync_DoesNotThrow()
    {
        var config = CreateConfiguration();
        var service = new EmailService(config);
        
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FullName = "Test User",
            CreatedAt = DateTime.UtcNow
        };

        await service.SendPasswordResetAsync(user, "resetToken123");
    }
}