using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel.Args;
using Service.Interfaces;

namespace Service.Features;

public sealed class MinioHandlerService : IMinioHandlerService
{
    private readonly IMinioClient _minioClient;

    public MinioHandlerService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<bool> WriteFileAsync(IFormFile file, string bucket, string obj)
    {
       bool flag = true;

        try
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var pubObjectArgs = new PutObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(obj)
                    .WithStreamData(new MemoryStream(fileBytes))
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType);

                await _minioClient.PutObjectAsync(pubObjectArgs).ConfigureAwait(false);

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("File Upload Error: {0}", ex.Message);
            flag = false;
        }
        return flag;
    }


    public async Task<bool> CreateBucketAsync(string name, string location)
    {
        bool flag = false;
        var beArgs = new BucketExistsArgs()
                .WithBucket(name);


        if (!await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false))
        {
            var mbArgs = new MakeBucketArgs()
                    .WithBucket(name)
                    .WithLocation(location);

            await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);

            flag = true;
        }

        return flag;
    }

    public async Task SetPolicyAsync(string bucket, string policy)
    {
        var bpArgs = new SetPolicyArgs()
                    .WithPolicy(policy)
                    .WithBucket(bucket);

        await _minioClient.SetPolicyAsync(bpArgs).ConfigureAwait(false);
    }

    public string GenerateFileName()
    {
        return Regex.Replace(Guid.NewGuid().ToString(), @"(\s+|-)", "");
    }
}