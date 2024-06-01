using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;

public interface IMinioHandlerService
{
    Task<bool> WriteFileAsync(IFormFile file, string bucket, string obj);
    Task<bool> CreateBucketAsync(string name, string location);
    Task SetPolicyAsync(string bucket, string policy);
    string GenerateFileName();


}