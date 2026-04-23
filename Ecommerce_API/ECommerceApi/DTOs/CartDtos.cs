using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.DTOs;

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class AddToCartDto
{
    [Required(ErrorMessage = "ProductId is required")]
    public int ProductId { get; set; }
    
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartQuantityDto
{
    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, 1000, ErrorMessage = "Quantity must be between 0 and 1000")]
    public int Quantity { get; set; }
}