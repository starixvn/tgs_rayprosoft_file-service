using Microsoft.AspNetCore.Http;

namespace FileService.Controllers.Messages
{
    public class UploadRequest
    {
        public IFormFile File { get; set; }
    }
}