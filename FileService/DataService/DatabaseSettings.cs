namespace FileService.DataService
{
    public interface IDatabaseSettings
    {
        string ConnectionString { get; init; }
    }

    public record DatabaseSettings : IDatabaseSettings
    {
        public string ConnectionString { get; init; }
    }
}