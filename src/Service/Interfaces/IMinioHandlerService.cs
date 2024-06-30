

namespace Service.Interfaces;

public interface IMinioHandlerService
{
    Task<bool> WriteFileAsync(Stream file, long length, string contentType, string bucket, string obj);
    Task<bool> CreateBucketAsync(string name, string location);
    Task SetPolicyAsync(string bucket, string policy);
    string GenerateFileName();
}