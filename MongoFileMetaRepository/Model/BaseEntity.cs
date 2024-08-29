using MongoDB.Bson.Serialization.Attributes;

namespace MongoFileMetaRepository.Model;

public abstract class BaseEntity
{
    [BsonId]
    public string Id { get; set; } = null!;
}