namespace FileService.Core;

public class AwsOptions
{
    public string AccessKeyId { get; set; } = default!;
    public string SecretAccessKey { get; set; } = default!;
    public string Region { get; set; } = default!;
    public string? ServiceUrl { get; set; }
    public bool UseHttp { get; set; }
    public S3 Public { get; set; } = default!;
    public S3 Private { get; set; } = default!;
}

public class S3
{
    public string ExternalServiceUrl { get; set; } = default!;
    public string ServiceUrl { get; set; } = default!;
    public string Bucket { get; set; } = default!;
}