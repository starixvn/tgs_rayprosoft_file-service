using FileService.DataService.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoFileMetaRepository;

namespace FileService.DataService.Services;

public sealed class PromotionService : IPromotionService
{
    private readonly IMongoContext<PromotionFile> _promotionRepository;

    public PromotionService(
        IMongoContext<PromotionFile> promotionRepository)
    {
        _promotionRepository = promotionRepository ??
            throw new ArgumentNullException(nameof(promotionRepository));
    }

    public async Task AddPromotionFileAsync(string id, string[] fileNames)
    {
        var filter = Builders<PromotionFile>.Filter.Eq(x => x.PromotionId, id);
        var promotionFile = await _promotionRepository.GetByFilterAsync(filter);
        if (promotionFile != null)
        {
            promotionFile.Files = fileNames;
            promotionFile.UpdatedOn = DateTimeOffset.Now;

            await _promotionRepository.UpdateAsync(promotionFile);
        }
        else
        {
            promotionFile = new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                PromotionId = id,
                Files = fileNames,
                CreatedOn = DateTimeOffset.Now,
                UpdatedOn = DateTimeOffset.Now
            };

            await _promotionRepository.InsertAsync(promotionFile);
        }
    }
}