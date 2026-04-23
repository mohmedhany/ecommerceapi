using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;

namespace ECommerceApi.Services;

public interface ICartService
{
    Task<IEnumerable<CartItemDto>> GetCartAsync(int userId);
    Task<CartItemDto> AddToCartAsync(int userId, AddToCartDto dto);
    Task<CartItemDto?> UpdateQuantityAsync(int userId, int productId, int quantity);
    Task<bool> RemoveFromCartAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
}

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    
    public CartService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<CartItemDto>> GetCartAsync(int userId)
    {
        return await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .Select(c => new CartItemDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product!.Name,
                ProductPrice = c.Product.Price,
                ImageUrl = c.Product.ImageUrl,
                Quantity = c.Quantity,
                TotalPrice = c.Product.Price * c.Quantity
            })
            .ToListAsync();
    }
    
    public async Task<CartItemDto> AddToCartAsync(int userId, AddToCartDto dto)
    {
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            throw new Exception("Product not found");
        
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);
        
        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            await _context.SaveChangesAsync();
            
            return new CartItemDto
            {
                Id = existingItem.Id,
                ProductId = existingItem.ProductId,
                ProductName = product.Name,
                ProductPrice = product.Price,
                ImageUrl = product.ImageUrl,
                Quantity = existingItem.Quantity,
                TotalPrice = product.Price * existingItem.Quantity
            };
        }
        
        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity
        };
        
        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();
        
        return new CartItemDto
        {
            Id = cartItem.Id,
            ProductId = cartItem.ProductId,
            ProductName = product.Name,
            ProductPrice = product.Price,
            ImageUrl = product.ImageUrl,
            Quantity = cartItem.Quantity,
            TotalPrice = product.Price * cartItem.Quantity
        };
    }
    
    public async Task<CartItemDto?> UpdateQuantityAsync(int userId, int productId, int quantity)
    {
        var cartItem = await _context.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        
        if (cartItem == null) return null;
        
        if (quantity <= 0)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return null;
        }
        
        cartItem.Quantity = quantity;
        await _context.SaveChangesAsync();
        
        return new CartItemDto
        {
            Id = cartItem.Id,
            ProductId = cartItem.ProductId,
            ProductName = cartItem.Product!.Name,
            ProductPrice = cartItem.Product.Price,
            ImageUrl = cartItem.Product.ImageUrl,
            Quantity = cartItem.Quantity,
            TotalPrice = cartItem.Product.Price * cartItem.Quantity
        };
    }
    
    public async Task<bool> RemoveFromCartAsync(int userId, int productId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        
        if (cartItem == null) return false;
        
        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    public async Task ClearCartAsync(int userId)
    {
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();
        
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();
    }
}