using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace FileService.Core
{
    public class FileManager<TAmazonS3> : IFileManager<TAmazonS3> where TAmazonS3 : IAmazonS3
    {
        private readonly TAmazonS3 _amazonS3;
        private readonly string _bucket;
        private readonly IFileMetaRepository _fileMetaRepository;
        private readonly string _serviceUrl;

        public FileManager(
            IOptions<AwsOptions> optionsAccessor,
            TAmazonS3 amazonS3,
            IFileMetaRepository fileMetaRepository)
        {
            var options = optionsAccessor.Value;
            _amazonS3 = amazonS3 ?? throw new ArgumentNullException(nameof(amazonS3));
            _fileMetaRepository = fileMetaRepository ?? throw new ArgumentNullException(nameof(fileMetaRepository));

            if (amazonS3.GetType().GetInterfaces().Contains(typeof(IPublicAmazonS3)))
            {
                _bucket = options.Public.Bucket;
                _serviceUrl = options.Public.ExternalServiceUrl;
            }
            else
            {
                _bucket = options.Private.Bucket;
                _serviceUrl = options.Private.ExternalServiceUrl;
            }
        }

        public async Task DeleteFilesAsync(IEnumerable<FileMeta> files)
        {
            await _fileMetaRepository
                .DeleteFileMetaByIdsAsync(files.Select(file => file.Id!))
                .ConfigureAwait(false);

            var request = new DeleteObjectsRequest
            {
                BucketName = _bucket,
                Objects = files.Select(file => new KeyVersion {Key = file.FileName}).ToList()
            };

            await _amazonS3.DeleteObjectsAsync(request).ConfigureAwait(false);
        }

        public IAsyncEnumerable<FileMeta> FindExpiredFilesAsync(string prefix, DateTimeOffset expireTime)
            => _fileMetaRepository.ListExpiredFileMetasAsync(prefix, expireTime);

        public async Task<FileMeta> GetFileMetaByObjectIdAsync(string objectId) =>
            await _fileMetaRepository.FindByIdAsync(objectId).ConfigureAwait(false);

        public IAsyncEnumerable<FileMeta> ListPathAsync(string prefix)
            => _fileMetaRepository.ListFileMetasAsync(prefix);

        public Task<Stream> OpenFileStreamAsync(FileMeta meta)
        {
            var url = _amazonS3.GeneratePreSignedURL(_bucket, meta.FileName, DateTime.UtcNow.AddMinutes(5), null);

            var client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate
            {
                return true;
            };

            return client.OpenReadTaskAsync(url);
        }

        public async Task<FileMeta> SaveAsync(string filePath, string contentType, long contentLength,
            Stream fileStream)
        {
            var savePath = filePath.TrimStart('/');

            var request = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = savePath,
                InputStream = fileStream
            };

            var response = await _amazonS3.PutObjectAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(
                    $"[FileService] Couldn't upload file {filePath}.");
            }

            var file = new FileMeta
            {
                FileName = savePath,
                ContentType = contentType,
                FileLength = contentLength,
                CreatedOn = DateTimeOffset.Now
            };

            var id = await _fileMetaRepository.AddFileMetaAsync(file).ConfigureAwait(false);

            file.Id = id;
            file.ExternalUrl = $"{_serviceUrl}/{_bucket}/{savePath}";

            return file;
        }
    }
}