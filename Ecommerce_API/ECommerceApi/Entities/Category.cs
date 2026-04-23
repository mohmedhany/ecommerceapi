using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.Entities;

public class Category
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}