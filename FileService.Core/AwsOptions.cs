namespace FileService.Core
{
    public class AwsOptions
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string ServiceUrl { get; set; }
        public bool UseHttp { get; set; }
        public S3 Public { get; set; }
        public S3 Private { get; set; }
    }

    public class S3
    {
        public string ExternalServiceUrl { get; set; }
        public string ServiceUrl { get; set; }
        public string Bucket { get; set; }
    }
}