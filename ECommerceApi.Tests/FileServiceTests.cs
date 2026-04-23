using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using ECommerceApi.DTOs;
using ECommerceApi.Services;

namespace ECommerceApi.Tests;

public class FileServiceTests
{
    private IConfiguration CreateConfiguration()
    {
        var dict = new Dictionary<string, string?>
        {
            { "AppUrl", "http://localhost:5000" }
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private Mock<IWebHostEnvironment> CreateMockEnvironment(string webRootPath)
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(e => e.WebRootPath).Returns(webRootPath);
        return mock;
    }

    [Fact]
    public void GetFileUrl_ReturnsCorrectUrl()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var mockEnv = CreateMockEnvironment(tempDir);
        var config = CreateConfiguration();
        var service = new FileService(mockEnv.Object, config);

        var result = service.GetFileUrl("test.jpg", "products");

        Assert.Contains("products", result);
        Assert.Contains("test.jpg", result);
    }

    [Fact]
    public async Task DeleteImageAsync_ReturnsFalse_WhenPathEmpty()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var mockEnv = CreateMockEnvironment(tempDir);
        var config = CreateConfiguration();
        var service = new FileService(mockEnv.Object, config);

        var result = await service.DeleteImageAsync("");

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteImageAsync_ReturnsFalse_WhenFileNotFound()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var mockEnv = CreateMockEnvironment(tempDir);
        var config = CreateConfiguration();
        var service = new FileService(mockEnv.Object, config);

        var result = await service.DeleteImageAsync("http://localhost:5000/uploads/nonexistent.jpg");

        Assert.False(result);
        
        Directory.Delete(tempDir, true);
    }
}