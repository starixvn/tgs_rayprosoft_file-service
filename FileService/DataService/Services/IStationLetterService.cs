namespace FileService.DataService.Services;

public interface IStationLetterService
{
    Task AddStationLetterFileAsync(string id, string[] fileNames);
}