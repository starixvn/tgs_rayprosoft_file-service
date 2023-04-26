using System.Net;
using Amazon.S3.Model;
using ExpectedObjects;
using FileService.Core;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace FileService.UnitTests.Services;

[TestFixture]
public class FileManagerTests
{
    [Test]
    public async Task Save_File_Should_Success()
    {
        var options = new AwsOptions
        {
            Private = new()
            {
                ExternalServiceUrl = "https://aws",
                Bucket = "bucket"
            }
        };
        var moqAmazonS3 = new Mock<IPrivateAmazonS3>();
        var moqFileMetaRepository = new Mock<IFileMetaRepository>();

        var sut = new FileManager<IPrivateAmazonS3>(
            new OptionsWrapper<AwsOptions>(options),
            moqAmazonS3.Object,
            moqFileMetaRepository.Object);

        var filePath = "/test/path/file.ext";
        var contentType = "testContentType";
        var fileLength = 1024L;
        using var fileStream = new MemoryStream();

        var expected = new FileMeta
        {
            Id = "testFileId",
            FileName = "test/path/file.ext",
            ContentType = contentType,
            FileLength = fileLength,
            CreatedOn = DateTimeOffset.UtcNow,
            ExternalUrl = $"{options.Private.ServiceUrl}/{options.Private.Bucket}/test/path/file.ext"
        };

        moqAmazonS3
            .Setup(fake => fake.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK }));
        moqFileMetaRepository
            .Setup(fake => fake.AddFileMetaAsync(
                It.IsAny<FileMeta>()))
            .Returns(ValueTask.FromResult(expected.Id));

        var actual = await sut.SaveAsync(filePath, contentType, fileLength, fileStream);

        expected.ToExpectedObject(ctx =>
        {
            ctx.Ignore(obj => obj.CreatedOn);
        }).ShouldMatch(actual);
    }
}