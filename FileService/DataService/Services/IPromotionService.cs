namespace FileService.DataService.Services;

public interface IPromotionService
{
    Task AddPromotionFileAsync(string id, string[] fileNames);
}