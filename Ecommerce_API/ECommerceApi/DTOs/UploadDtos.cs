namespace ECommerceApi.DTOs;

public class UploadResponse
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}