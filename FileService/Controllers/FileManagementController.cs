using System;
using System.Linq;
using System.Threading.Tasks;
using FileService.Controllers.Messages;
using FileService.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers
{
    [Route("api/fs")]
    [ApiController]
    public class FileManagementController : ControllerBase
    {
        private readonly IFileManager<IPrivateAmazonS3> _fileManager;

        public FileManagementController(IFileManager<IPrivateAmazonS3> fileManager)
        {
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        }

        [HttpGet("{*path}")]
        public async Task<IActionResult> GetFileAsync(
            string path)
        {
            var meta = await _fileManager.ListPathAsync(path)
                .FirstAsync()
                .ConfigureAwait(false);

            var fileStream = await _fileManager.OpenFileStreamAsync(meta).ConfigureAwait(false);

            return File(fileStream, meta.ContentType);
        }

        [HttpGet("~/api/meta/{objectId}")]
        public async Task<FileViewModel> GetFileMetaAsync(string objectId)
        {
            var meta = await _fileManager.GetFileMetaByObjectIdAsync(objectId).ConfigureAwait(false);

            return new()
            {
                Id = meta.Id,
                FileName = meta.FileName,
                ContentType = meta.ContentType,
                FileLength = meta.FileLength,
                CreatedOn = meta.CreatedOn,
                ExternalUrl = meta.ExternalUrl
            };
        }

        [HttpPost("{*path}")]
        public async Task<FileViewModel> UploadFileAsync(
            string path,
            [FromForm(Name = "file")] IFormFile file)
        {
            var meta = await _fileManager
                .SaveAsync(path, file.ContentType, file.Length, file.OpenReadStream())
                .ConfigureAwait(false);

            return new()
            {
                Id = meta.Id,
                FileName = meta.FileName,
                ContentType = meta.ContentType,
                FileLength = meta.FileLength,
                CreatedOn = meta.CreatedOn,
                ExternalUrl = meta.ExternalUrl
            };
        }
    }
}
