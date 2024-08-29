namespace FileService.Controllers.Messages;

public record FileViewModel
{
    public string Id { get; init; } = null!;

    public string FileName { get; init; } = null!;

    public string ContentType { get; init; } = null!;

    public decimal FileLength { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public string ExternalUrl { get; init; } = null!;
}