using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApi.Entities;

public class CartItem
{
    [Key]
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int ProductId { get; set; }
    
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
    
    public int Quantity { get; set; } = 1;
}