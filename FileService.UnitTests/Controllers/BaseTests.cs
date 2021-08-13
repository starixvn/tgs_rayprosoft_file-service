using System.IO;
using System.Linq;
using FileService.Controllers.Messages.Validators;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FileService.UnitTests.Controllers
{
    public class BaseTests
    {
        protected readonly Mock<IFormFile> fileMock = new Mock<IFormFile>();

        protected void CreateFile(string extension = ".jpeg")
        {
            var fileSignature = ImageFileSignature.FileSignature[extension];
            //Setup mock file using a memory stream
            var fileName = $"test.{extension}";
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            writer.Write(fileSignature.SelectMany(x => x).ToArray());
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
        }

        protected void CreateWrongSignatureFile()
        {
            //Setup mock file using a memory stream
            var content = "this is wrong file.";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);

            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
        }

        protected void CreateGreater2MFile()
        {
            //Setup mock file using a memory stream
            var byDataValue = new char[3097152];
            var fileName = "test.jpeg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(byDataValue);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
        }
    }
}