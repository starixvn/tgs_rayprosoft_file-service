using FileService.Core;

namespace FileService.DataService.Services;

public interface IUploadService
{
    Task<FileMeta> UploadFileS3Async(string prefix, IFormFile formFile);

    ValueTask<string[]> GetObjectsAsync(string prefix = null);
}