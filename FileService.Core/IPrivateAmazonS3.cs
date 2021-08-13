using Amazon.S3;

namespace FileService.Core
{
    public interface IPrivateAmazonS3 : IAmazonS3
    {
    }

    public class PrivateAmazonS3Client : AmazonS3Client, IPrivateAmazonS3
    {
        public PrivateAmazonS3Client(
            string awsAccessKeyId,
            string awsSecretAccessKey,
            AmazonS3Config clientConfig) : base(awsAccessKeyId, awsSecretAccessKey, clientConfig)
        {
        }
    }
}