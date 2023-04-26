namespace FileService.Controllers.Messages;

public record FileViewModel
{
    public string Id { get; init; }

    public string FileName { get; init; }

    public string ContentType { get; init; }

    public decimal FileLength { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public string ExternalUrl { get; init; }
}