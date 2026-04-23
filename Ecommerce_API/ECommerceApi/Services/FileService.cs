using Microsoft.AspNetCore.Http;
using ECommerceApi.DTOs;

namespace ECommerceApi.Services;

public interface IFileService
{
    Task<UploadResponse> UploadImageAsync(IFormFile file, string folder);
    Task<bool> DeleteImageAsync(string filePath);
    Task<List<UploadResponse>> UploadMultipleAsync(IFormFileCollection files, string folder);
    string GetFileUrl(string fileName, string folder);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    
    public FileService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }
    
    public async Task<UploadResponse> UploadImageAsync(IFormFile file, string folder)
    {
        if (file.Length == 0)
            throw new Exception("File is empty");
        
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        
        if (!allowedExtensions.Contains(fileExtension))
            throw new Exception("Invalid file type. Allowed: jpg, jpeg, png, gif, webp");
        
        if (file.Length > 5 * 1024 * 1024) // 5MB max
            throw new Exception("File size must be less than 5MB");
        
        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", folder);
        
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);
        
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        var baseUrl = _configuration["AppUrl"] ?? "http://localhost:5000";
        
        return new UploadResponse
        {
            FileName = uniqueFileName,
            FilePath = $"{baseUrl}/uploads/{folder}/{uniqueFileName}",
            FileSize = FormatFileSize(file.Length),
            ContentType = file.ContentType
        };
    }
    
    public async Task<List<UploadResponse>> UploadMultipleAsync(IFormFileCollection files, string folder)
    {
        var responses = new List<UploadResponse>();
        
        foreach (var file in files)
        {
            var response = await UploadImageAsync(file, folder);
            responses.Add(response);
        }
        
        return responses;
    }
    
    public async Task<bool> DeleteImageAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            
            var url = new Uri(filePath);
            var fileName = Path.GetFileName(url.LocalPath);
            var fullPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", fileName);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    public string GetFileUrl(string fileName, string folder)
    {
        var baseUrl = _configuration["AppUrl"] ?? "http://localhost:5000";
        return $"{baseUrl}/uploads/{folder}/{fileName}";
    }
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }
}