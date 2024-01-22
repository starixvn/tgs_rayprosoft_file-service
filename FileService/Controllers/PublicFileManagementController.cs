using FileService.Controllers.Messages;
using FileService.Core;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers;

[Route("api/fs/public")]
[ApiController]
public class PublicFileManagementController : ControllerBase
{
    private readonly IFileManager<IPublicAmazonS3> _fileManager;

    public PublicFileManagementController(IFileManager<IPublicAmazonS3> fileManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
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
            Id = meta.Id!,
            FileName = meta.FileName,
            ContentType = meta.ContentType,
            FileLength = meta.FileLength,
            CreatedOn = meta.CreatedOn,
            ExternalUrl = meta.ExternalUrl!
        };
    }
}