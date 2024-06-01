using Service.Dtos;
using Service.Interfaces;

namespace Service.Repositories;

public sealed class FileManagerService : IFileManagerService
{
    
    private readonly IMinioHandlerService _minionHandler;

    public FileManagerService(IMinioHandlerService minionHandler)
    {
        _minionHandler = minionHandler;
    }

    public async Task<ResponseDto> Upload(FileRequestDto files, string location)
    {
        List<string> finalUrls = new();
        Policy.Bucket = files.Bucket;

        if(await _minionHandler.CreateBucketAsync(files.Bucket, location))
            await _minionHandler.SetPolicyAsync(files.Bucket, Policy.ReadPolicy);

        foreach (var file in files.Files)
        {
            string fileName = string.Concat(_minionHandler.GenerateFileName(), Path.GetExtension(file.FileName));
            string obj = string.Concat(files.Directory, '/', fileName);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();


                if(await _minionHandler.WriteFileAsync(
                    file: new MemoryStream(fileBytes), 
                    length: file.Length,
                    contentType: file.ContentType,
                    bucket: files.Bucket,
                    obj: obj
                ))
                    finalUrls.Add(string.Concat(files.Bucket, '/', obj));
            }
        }

        finalUrls.TryGetNonEnumeratedCount(out int count);

        return new ResponseDto
        {
            IsSuccess = count != 0,
            Result = finalUrls
        };
    }
}