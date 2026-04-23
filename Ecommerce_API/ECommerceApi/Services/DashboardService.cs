using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;

namespace ECommerceApi.Services;

public class DashboardService
{
    private readonly AppDbContext _context;
    
    public DashboardService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<object> GetDashboardStats()
    {
        var totalProducts = await _context.Products.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
        
        return new 
        {
            totalProducts,
            totalOrders,
            totalUsers,
            totalRevenue,
            currency = "EGP"
        };
    }
    
    public async Task<List<RevenueReport>> GetRevenueReport(DateTime startDate, DateTime endDate)
    {
        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync();
            
        var grouped = orders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new RevenueReport
            {
                Date = g.Key,
                TotalOrders = g.Count(),
                TotalRevenue = g.Sum(o => o.TotalAmount)
            })
            .OrderBy(r => r.Date)
            .ToList();
            
        return grouped;
    }
    
    public async Task<List<TopProductDto>> GetTopProducts(int count = 10)
    {
        var orderItems = await _context.OrderItems
            .Include(oi => oi.Product)
            .ToListAsync();
            
        var grouped = orderItems
            .GroupBy(oi => new { oi.ProductId, oi.Product?.Name })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name ?? "Unknown",
                UnitsSold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
            })
            .OrderByDescending(p => p.UnitsSold)
            .Take(count)
            .ToList();
            
        return grouped;
    }
    
    public async Task<List<InventoryReport>> GetLowStockProducts(int threshold = 10)
    {
        var products = await _context.Products
            .Where(p => p.Stock <= threshold)
            .Select(p => new InventoryReport
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CurrentStock = p.Stock,
                Price = p.Price
            })
            .OrderBy(p => p.CurrentStock)
            .ToListAsync();
            
        return products;
    }
    
    public async Task<object> GetOrdersStats()
    {
        var stats = await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new 
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync();
            
        return stats;
    }
    
    public async Task<List<UserStats>> GetUserStats()
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        
        var newUsers = await _context.Users
            .Where(u => u.CreatedAt >= thirtyDaysAgo)
            .CountAsync();
            
        var totalUsers = await _context.Users.CountAsync();
        
        return new List<UserStats>
        {
            new UserStats { Label = "Total Users", Count = totalUsers },
            new UserStats { Label = "New Users (30 days)", Count = newUsers }
        };
    }
}

public class RevenueReport
{
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}

public class InventoryReport
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public decimal Price { get; set; }
}

public class UserStats
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}