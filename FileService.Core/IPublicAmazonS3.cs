using Amazon.S3;

namespace FileService.Core
{
    public interface IPublicAmazonS3 : IAmazonS3
    {
    }

    public class PublicAmazonS3Client : AmazonS3Client, IPublicAmazonS3
    {
        public PublicAmazonS3Client(
            string awsAccessKeyId,
            string awsSecretAccessKey,
            AmazonS3Config clientConfig) : base(awsAccessKeyId, awsSecretAccessKey, clientConfig)
        {
        }
    }
}