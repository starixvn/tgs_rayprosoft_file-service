using System.Threading.Tasks;
using FileService.Core;
using Microsoft.AspNetCore.Http;

namespace FileService.DataService.Services
{
    public interface IUploadService
    {
        Task<FileMeta> UploadFileS3Async(string prefix, IFormFile formFile);

        ValueTask<string[]> GetObjectsAsync(string prefix = null);
    }
}