namespace FileService.Core
{
    public class AwsOptions
    {
        public string AccessKeyId { get; init; }
        public string SecretAccessKey { get; init; }
        public string Region { get; init; }
        public string ServiceUrl { get; init; }
        public bool UseHttp { get; init; }
        public S3 Public { get; init; }
        public S3 Private { get; init; }
    }

    public record S3
    {
        public string ExternalServiceUrl { get; init; }
        public string ServiceUrl { get; init; }
        public string Bucket { get; init; }
    }
}