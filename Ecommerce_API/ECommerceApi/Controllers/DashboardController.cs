using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Services;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;
    
    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
    
    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }
    
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _dashboardService.GetDashboardStats();
        return Ok(stats);
    }
    
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var report = await _dashboardService.GetRevenueReport(startDate, endDate);
        return Ok(report);
    }
    
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int count = 10)
    {
        var products = await _dashboardService.GetTopProducts(count);
        return Ok(products);
    }
    
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
    {
        var products = await _dashboardService.GetLowStockProducts(threshold);
        return Ok(products);
    }
    
    [HttpGet("orders-stats")]
    public async Task<IActionResult> GetOrdersStats()
    {
        var stats = await _dashboardService.GetOrdersStats();
        return Ok(stats);
    }
    
    [HttpGet("users-stats")]
    public async Task<IActionResult> GetUsersStats()
    {
        var stats = await _dashboardService.GetUserStats();
        return Ok(stats);
    }
}