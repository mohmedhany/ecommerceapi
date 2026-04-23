using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApi.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    public int Stock { get; set; }
    
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}