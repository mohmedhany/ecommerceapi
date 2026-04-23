using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.DTOs;
using ECommerceApi.Services;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    
    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        var userId = GetUserId();
        var item = await _cartService.AddToCartAsync(userId, dto);
        return Ok(item);
    }
    
    [Authorize]
    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] UpdateCartQuantityDto dto)
    {
        var userId = GetUserId();
        var item = await _cartService.UpdateQuantityAsync(userId, productId, dto.Quantity);
        if (item == null) return NotFound();
        return Ok(item);
    }
    
    [Authorize]
    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var userId = GetUserId();
        var result = await _cartService.RemoveFromCartAsync(userId, productId);
        if (!result) return NotFound();
        return NoContent();
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }
}