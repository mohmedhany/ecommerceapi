using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;

namespace ECommerceApi.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(int page, int pageSize);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductDto>> SearchAsync(string query);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalCountAsync();
}

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    
    public ProductService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<ProductDto>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category!.Name
            })
            .ToListAsync();
    }
    
    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Products.CountAsync();
    }
    
    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (product == null) return null;
        
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category!.Name
        };
    }
    
    public async Task<IEnumerable<ProductDto>> SearchAsync(string query)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category!.Name
            })
            .ToListAsync();
    }
    
    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category!.Name
            })
            .ToListAsync();
    }
    
    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl ?? string.Empty,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(product.Id) ?? throw new Exception("Error creating product");
    }
    
    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return null;
        
        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.ImageUrl != null) product.ImageUrl = dto.ImageUrl;
        
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(id);
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;
        
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        return true;
    }
}