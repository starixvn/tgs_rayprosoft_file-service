using FileService.DataService.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoFileMetaRepository;

namespace FileService.DataService.Services;

public sealed class StationLetterService : IStationLetterService
{
    private readonly IMongoContext<StationLetterFile> _stationLetterRepository;

    public StationLetterService(
        IMongoContext<StationLetterFile> stationLetterRepository)
    {
        _stationLetterRepository = stationLetterRepository ??
            throw new ArgumentNullException(nameof(stationLetterRepository));
    }

    public async Task AddStationLetterFileAsync(string id, string[] fileNames)
    {
        var filter = Builders<StationLetterFile>.Filter.Eq(x => x.StationLetterId, id);
        var stationLetterFile = await _stationLetterRepository.GetByFilterAsync(filter);
        if (stationLetterFile != null)
        {
            stationLetterFile.Files = fileNames;
            stationLetterFile.UpdatedOn = DateTimeOffset.Now;

            await _stationLetterRepository.UpdateAsync(stationLetterFile);
        }
        else
        {
            stationLetterFile = new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                StationLetterId = id,
                Files = fileNames,
                CreatedOn = DateTimeOffset.Now,
                UpdatedOn = DateTimeOffset.Now
            };

            await _stationLetterRepository.InsertAsync(stationLetterFile);
        }
    }
}