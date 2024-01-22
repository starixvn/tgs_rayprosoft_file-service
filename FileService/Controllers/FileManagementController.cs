using FileService.Controllers.Messages;
using FileService.Core;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers;

[Route("api/fs")]
[ApiController]
public class FileManagementController(IFileManager<IPrivateAmazonS3> fileManager) : ControllerBase
{
    [HttpGet("{*path}")]
    public async Task<IActionResult> GetFileAsync(
        string? path)
    {
        var meta = await fileManager.ListPathAsync(path)
            .FirstAsync()
            .ConfigureAwait(false);

        var fileStream = await fileManager.OpenFileStreamAsync(meta).ConfigureAwait(false);

        return File(fileStream, meta.ContentType);
    }

    [HttpGet("~/api/meta/{objectId}")]
    public async Task<FileViewModel> GetFileMetaAsync(string objectId)
    {
        var meta = await fileManager.GetFileMetaByObjectIdAsync(objectId).ConfigureAwait(false);

        return new()
        {
            Id = meta.Id!,
            FileName = meta.FileName,
            ContentType = meta.ContentType,
            FileLength = meta.FileLength,
            CreatedOn = meta.CreatedOn,
            ExternalUrl = meta.ExternalUrl!
        };
    }

    [HttpPost("{*path}")]
    public async Task<FileViewModel> UploadFileAsync(
        string path,
        [FromForm(Name = "file")] IFormFile file)
    {
        var meta = await fileManager
            .SaveAsync(path, file.ContentType, file.Length, file.OpenReadStream())
            .ConfigureAwait(false);

        return new()
        {
            Id = meta.Id!,
            FileName = meta.FileName,
            ContentType = meta.ContentType,
            FileLength = meta.FileLength,
            CreatedOn = meta.CreatedOn,
            ExternalUrl = meta.ExternalUrl!
        };
    }
}