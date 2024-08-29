using MongoFileMetaRepository.Model;

namespace FileService.DataService.Model;

public class PromotionFile : BaseEntity
{
    public string PromotionId { get; set; } = null!;

    public ICollection<string> Files { get; set; } = null!;

    public DateTimeOffset CreatedOn { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }
}