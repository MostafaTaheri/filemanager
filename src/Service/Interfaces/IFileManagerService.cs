using Service.Dtos;

namespace Service.Interfaces;

public interface IFileManagerService
{
    Task<ResponseDto> Upload(FileRequestDto files, string location);
}