using FileService.Core;
using Tgs.Core.Infrastructures.Utility;

namespace FileService.DataService.Services;

public sealed class UploadService : IUploadService
{
    private readonly IFileManager<IPublicAmazonS3> _fileManager;

    public UploadService(
        IFileManager<IPublicAmazonS3> fileManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
    }

    public async Task<FileMeta> UploadFileS3Async(string prefix, IFormFile formFile)
    {
        var randomFileName =
            $"{StringGenerator.GenerateAlphanumericString(32)}{Path.GetExtension(formFile.FileName)}";

        return await _fileManager
            .SaveAsync($"{prefix}/{randomFileName}", formFile.ContentType, formFile.Length,
                formFile.OpenReadStream())
            .ConfigureAwait(false);
    }

    public ValueTask<string[]> GetObjectsAsync(string prefix)
        => _fileManager.ListPathAsync(prefix)
            .Select(meta => meta.FileName)
            .ToArrayAsync();
}