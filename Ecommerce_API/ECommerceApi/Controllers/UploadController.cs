using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Services;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IFileService _fileService;
    
    public UploadController(IFileService fileService)
    {
        _fileService = fileService;
    }
    
    [Authorize]
    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "products")
    {
        try
        {
            var result = await _fileService.UploadImageAsync(file, folder);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [Authorize]
    [HttpPost("images")]
    public async Task<IActionResult> UploadImages(IFormFileCollection files, [FromQuery] string folder = "products")
    {
        try
        {
            var result = await _fileService.UploadMultipleAsync(files, folder);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> DeleteImage([FromQuery] string filePath)
    {
        var result = await _fileService.DeleteImageAsync(filePath);
        if (!result) return NotFound();
        return NoContent();
    }
}