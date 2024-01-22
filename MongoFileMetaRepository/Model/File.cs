namespace MongoFileMetaRepository.Model;

public class File : BaseEntity
{
    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public decimal FileLength { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
}