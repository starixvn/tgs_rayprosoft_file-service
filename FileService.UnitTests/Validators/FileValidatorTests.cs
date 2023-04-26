using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using FileService.Controllers.Messages;
using FileService.Controllers.Messages.Validators;
using FileService.UnitTests.Controllers;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using File = TGS.Apis.FileService.Resources.File;

namespace FileService.UnitTests.Validators;

[TestFixture]
public class FileValidatorTests : BaseTests
{
    [SetUp]
    public void Setup()
    {
        var settings = Customize<FileSettings>(x => x).Create<FileSettings>();
        settings.FileSizeLimit = 2097152;
        _settingsMock.Setup(x => x.Value).Returns(settings);
        _validator = new(_settingsMock.Object);
    }

    private readonly Mock<IOptions<FileSettings>> _settingsMock = new();
    private FileValidator _validator;

    [Test]
    public void Should_have_error_when_file_size_greater_2MB()
    {
        CreateGreater2MFile();

        var model = new UploadRequest { File = fileMock.Object };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(request => request.File.Length)
            .WithErrorMessage(File.FileMaxLength);
    }

    [Test]
    public void Should_have_not_error_when_upload_file()
    {
        CreateFile();

        var model = new UploadRequest { File = fileMock.Object };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.File.Length);
    }

    [Test]
    public void Should_have_error_when_wrong_file_extension()
    {
        CreateWrongSignatureFile();

        var model = new UploadRequest { File = fileMock.Object };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(request => request.File)
            .WithErrorMessage(File.InvalidImageSignature);
    }

    [TestCase(".jpeg")]
    [TestCase(".jpg")]
    [TestCase(".png")]
    [TestCase(".gif")]
    public void Should_have_not_error_when_right_file_extension(string extension)
    {
        CreateFile(extension);

        var model = new UploadRequest { File = fileMock.Object };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.File);
    }

    private Fixture Customize<T>(Func<ICustomizationComposer<T>, ISpecimenBuilder> composerTransformation)
        where T : new()
    {
        var fixture = new Fixture();
        fixture.Customize(composerTransformation);
        return fixture;
    }
}