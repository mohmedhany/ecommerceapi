using Microsoft.EntityFrameworkCore;
using Moq;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;
using ECommerceApi.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceApi.Tests;

public class AuthServiceTests
{
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salted = password + "ECommerceSalt2024";
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(salted));
        return Convert.ToBase64String(hash);
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private IConfiguration CreateConfiguration()
    {
        var dict = new Dictionary<string, string?>
        {
            { "Jwt:Key", "ThisIsASecretKeyForJwtTokenGeneration12345" },
            { "Jwt:Issuer", "ECommerceApi" },
            { "Jwt:Audience", "ECommerceApiUsers" }
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public async Task RegisterAsync_ReturnsToken_WhenSuccessful()
    {
        using var context = CreateContext();
        var mockEmailService = new Mock<IEmailService>();
        var config = CreateConfiguration();
        
        var service = new AuthService(context, config, mockEmailService.Object);
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "password123",
            FullName = "Test User"
        };

        var result = await service.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEmailExists()
    {
        using var context = CreateContext();
        var mockEmailService = new Mock<IEmailService>();
        var config = CreateConfiguration();
        
        var existingUser = new User
        {
            Id = 1,
            Email = "test@test.com",
            FullName = "Existing User",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(existingUser);
        context.SaveChanges();
        
        var service = new AuthService(context, config, mockEmailService.Object);
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "password123",
            FullName = "Test User"
        };

        await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenValidCredentials()
    {
        using var context = CreateContext();
        var mockEmailService = new Mock<IEmailService>();
        var config = CreateConfiguration();
        
        var correctHash = HashPassword("password123");
        
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FullName = "Test User",
            PasswordHash = correctHash,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        context.SaveChanges();
        
        var service = new AuthService(context, config, mockEmailService.Object);
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "password123"
        };

        var result = await service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenUserNotFound()
    {
        using var context = CreateContext();
        var mockEmailService = new Mock<IEmailService>();
        var config = CreateConfiguration();
        
        var service = new AuthService(context, config, mockEmailService.Object);
        var request = new LoginRequest
        {
            Email = "notfound@test.com",
            Password = "password123"
        };

        await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenInvalidPassword()
    {
        using var context = CreateContext();
        var mockEmailService = new Mock<IEmailService>();
        var config = CreateConfiguration();
        
        var correctHash = HashPassword("password123");
        
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FullName = "Test User",
            PasswordHash = correctHash,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        context.SaveChanges();
        
        var service = new AuthService(context, config, mockEmailService.Object);
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "wrongpassword"
        };

        await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(request));
    }

    [Fact]
    public async Task SocialLoginAsync_CreatesNewUser_WhenNotExists()
    {
        using var context = CreateContext();
        var mockEmailService = new Mock<IEmailService>();
        var config = CreateConfiguration();
        
        var service = new AuthService(context, config, mockEmailService.Object);

        var result = await service.SocialLoginAsync("Google", "provider123", "new@test.com", "New User");

        Assert.NotNull(result);
        Assert.Equal("new@test.com", result.Email);
    }

    [Fact]
    public async Task SocialLoginAsync_ReturnsToken_WhenUserExists()
    {
        using var context = CreateContext();
        var mockEmailService = new Mock<IEmailService>();
        var config = CreateConfiguration();
        
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FullName = "Test User",
            Provider = "Google",
            ProviderKey = "provider123",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        context.SaveChanges();
        
        var service = new AuthService(context, config, mockEmailService.Object);

        var result = await service.SocialLoginAsync("Google", "provider123", "test@test.com", "Test User");

        Assert.NotNull(result);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser_WhenExists()
    {
        using var context = CreateContext();
        var mockEmailService = new Mock<IEmailService>();
        var config = CreateConfiguration();
        
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FullName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        context.SaveChanges();
        
        var service = new AuthService(context, config, mockEmailService.Object);

        var result = await service.GetUserByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContext();
        var mockEmailService = new Mock<IEmailService>();
        var config = CreateConfiguration();
        
        var service = new AuthService(context, config, mockEmailService.Object);

        var result = await service.GetUserByIdAsync(999);

        Assert.Null(result);
    }
}