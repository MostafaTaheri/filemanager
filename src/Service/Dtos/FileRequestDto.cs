using Microsoft.AspNetCore.Http;

namespace Service.Dtos;


public sealed class FileRequestDto
{
    public required IEnumerable<IFormFile> Files { get; set; }
    public required string Bucket { get; set; }
    public string? Directory { get; set; }
}
