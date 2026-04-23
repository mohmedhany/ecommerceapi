using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;

namespace ECommerceApi.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<bool> DeleteAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    
    public CategoryService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.Products)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductsCount = c.Products.Count
            })
            .ToListAsync();
    }
    
    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (category == null) return null;
        
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ProductsCount = category.Products.Count
        };
    }
    
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description ?? string.Empty
        };
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ProductsCount = 0
        };
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;
        
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        
        return true;
    }
}