using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;

namespace ECommerceApi.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetOrdersAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto);
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    
    public OrderService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<OrderDto>> GetOrdersAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserEmail = o.User!.Email,
                ShippingAddress = o.ShippingAddress,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product!.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.UnitPrice * oi.Quantity
                }).ToList()
            })
            .ToListAsync();
    }
    
    public async Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        
        if (order == null) return null;
        
        return new OrderDto
        {
            Id = order.Id,
            UserEmail = order.User!.Email,
            ShippingAddress = order.ShippingAddress,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product!.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.UnitPrice * oi.Quantity
            }).ToList()
        };
    }
    
    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();
        
        if (!cartItems.Any())
            throw new Exception("Cart is empty");
        
        var totalAmount = cartItems.Sum(c => c.Product!.Price * c.Quantity);
        
        var order = new Order
        {
            UserId = userId,
            ShippingAddress = dto.ShippingAddress,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        foreach (var item in cartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product!.Price
            };
            _context.OrderItems.Add(orderItem);
            
            item.Product.Stock -= item.Quantity;
        }
        
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();
        
        return await GetOrderByIdAsync(userId, order.Id) ?? throw new Exception("Error creating order");
    }
}