using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Service.Dtos;
using Service.Interfaces;

namespace Service.Repositories;

public sealed class FileManagerService : IFileManagerService
{
    private readonly IMinioClient _minioClient;

    public FileManagerService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<ResponseDto> Upload(FileRequestDto files, string location)
    {
        List<string> finalUrls = new();

        try
        {
            Policy.Bucket = files.Bucket;

            bool createdBucket = await CreateBucket(files.Bucket, location);

            if (createdBucket)
                await SetPolicy(files.Bucket, Policy.ReadPolicy);

            foreach (var file in files.Files)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    string fileName = string.Concat(GenerateFileName(), Path.GetExtension(file.FileName));
                    string _object = string.Concat(files.Directory, '/', fileName);

                    var pubObjectArgs = new PutObjectArgs()
                        .WithBucket(files.Bucket)
                        .WithObject(_object)
                        .WithStreamData(new MemoryStream(fileBytes))
                        .WithObjectSize(file.Length)
                        .WithContentType(file.ContentType);

                    await _minioClient.PutObjectAsync(pubObjectArgs).ConfigureAwait(false);

                    finalUrls.Add(string.Concat(files.Bucket, '/', _object));
                }
            }

           
        }
        // catch (MinioException ex)
        // {
        //     Console.WriteLine("File Upload Error: {0}", ex.Message);
        // }
        catch (Exception ex)
        {
            Console.WriteLine("File Upload Error: {0}", ex.Message);
        }

        return new ResponseDto
        {
            IsSuccess = true,
            Result = finalUrls
        };
    }


    private async Task<bool> CreateBucket(string name, string location)
    {
        var beArgs = new BucketExistsArgs()
                .WithBucket(name);


        if (!await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false))
        {
            var mbArgs = new MakeBucketArgs()
                    .WithBucket(name)
                    .WithLocation(location);

            await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
            return true;
        }

        return false;
    }

    private async Task SetPolicy(string bucket, string policy)
    {
        var bpArgs = new SetPolicyArgs()
                    .WithPolicy(policy)
                    .WithBucket(bucket);

        await _minioClient.SetPolicyAsync(bpArgs).ConfigureAwait(false);
    }

    private string GenerateFileName()
    {
        return Regex.Replace(Guid.NewGuid().ToString(), @"(\s+|-)", "");
    }
}