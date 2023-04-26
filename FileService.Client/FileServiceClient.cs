using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Tgs.FileService.Clients;

internal class FileServiceClient : IFileServiceClient
{
    private readonly HttpClient _httpClient;

    public FileServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async ValueTask<FileView> FindFileByObjectIdAsync(string objectId)
    {
        var response = await _httpClient
            .GetAsync($"/api/meta/{objectId}")
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var view = JsonSerializer.Deserialize<FileView>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        return view;
    }

    public async ValueTask<Stream> OpenFileReaderAsync(string path) =>
        await _httpClient
            .GetStreamAsync($"/api/fs/{path.TrimStart('/')}")
            .ConfigureAwait(false);

    public async ValueTask<FileView> UploadFileAsync(string path, MediaTypeHeaderValue contentType,
        Stream fileStream)
    {
        using var uploadContent = new MultipartFormDataContent();

        using var fileContent = new StreamContent(fileStream);

        var fileName = path.Split('/').Last();
        fileContent.Headers.ContentType = contentType;

        uploadContent.Add(fileContent, "file", fileName);

        var response = await _httpClient
            .PostAsync($"/api/fs/{path.TrimStart('/')}", uploadContent)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var view = JsonSerializer.Deserialize<FileView>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        return view;
    }

    public async ValueTask<FileView> UploadPublicFileAsync(string path, MediaTypeHeaderValue contentType,
        Stream fileStream)
    {
        using var uploadContent = new MultipartFormDataContent();

        using var fileContent = new StreamContent(fileStream);

        var fileName = path.Split('/').Last();
        fileContent.Headers.ContentType = contentType;

        uploadContent.Add(fileContent, "file", fileName);

        var response = await _httpClient
            .PostAsync($"/api/fs/public/{path.TrimStart('/')}", uploadContent)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var view = JsonSerializer.Deserialize<FileView>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        return view;
    }

    public async Task UpdateStationLetterFileAsync(string uploadType, string id, UpdateFilesRequest request)
    {
        var httpContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8,
            "application/json");

        await _httpClient
            .PutAsync($"/fs/file/{uploadType}/{id}", httpContent)
            .ConfigureAwait(false);
    }
}