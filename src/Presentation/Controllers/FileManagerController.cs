

using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Service.Dtos;
using Service.Interfaces;

namespace FileManager.Controllers;

[Route("api/v{version:apiVersion}/[Controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private IFileManagerService fileManagerService;

    public FileController(IFileManagerService fileManagerService)
    {
        this.fileManagerService = fileManagerService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    public async Task<ResponseDto> UploadFile([FromForm]FileRequestDto files)
    {
        return await fileManagerService.Upload(files , "us-east-1");
    }
}