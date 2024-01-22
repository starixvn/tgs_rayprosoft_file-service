namespace FileService.Core;

public interface IFileManager<TAmazonS3>
{
    Task<FileMeta> GetFileMetaByObjectIdAsync(string objectId);

    Task<Stream> OpenFileStreamAsync(FileMeta meta);

    Task<FileMeta> SaveAsync(string filePath, string contentType, long contentLength, Stream fileStream);

    IAsyncEnumerable<FileMeta> ListPathAsync(string? prefix);

    Task DeleteFilesAsync(IEnumerable<FileMeta> files);

    IAsyncEnumerable<FileMeta> FindExpiredFilesAsync(string prefix, DateTimeOffset expireTime);
}