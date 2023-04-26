using FileService.Core;
using FileService.DataService.Services;
using FileService.UnitTests.Controllers;
using Moq;
using NUnit.Framework;

namespace FileService.UnitTests.Services;

public class UploadServiceTests : BaseTests
{
    [Test]
    public async Task Can_Upload_S3()
    {
        CreateFile();

        var moqFileManager = new Mock<IFileManager<IPublicAmazonS3>>();

        var sut = new UploadService(moqFileManager.Object);

        var file = fileMock.Object;
        var expected = "https://aws/bucket/filepath";
        const string prefix = "prefix";

        moqFileManager
            .Setup(fake => fake.SaveAsync(
                It.IsAny<string>(),
                It.Is<string>(x => x == file.ContentType),
                It.Is<long>(x => x == file.Length),
                It.IsAny<Stream>()))
            .Returns(Task.FromResult(new FileMeta
            {
                Id = "testId",
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileLength = file.Length,
                CreatedOn = DateTimeOffset.Now,
                ExternalUrl = expected
            }));

        var fileMeta = await sut.UploadFileS3Async(prefix, file);

        Assert.AreEqual(expected, fileMeta.ExternalUrl);
    }
}