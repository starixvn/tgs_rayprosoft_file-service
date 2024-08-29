using MongoFileMetaRepository.Model;

namespace FileService.DataService.Model;

public class StationLetterFile : BaseEntity
{
    public string StationLetterId { get; set; } = null!;

    public ICollection<string> Files { get; set; } = null!;

    public DateTimeOffset CreatedOn { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }
}