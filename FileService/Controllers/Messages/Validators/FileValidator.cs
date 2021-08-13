using FluentValidation;
using Microsoft.Extensions.Options;
using TGS.Apis.FileService.Resources;

namespace FileService.Controllers.Messages.Validators
{
    public class FileValidator : AbstractValidator<UploadRequest>
    {
        public FileValidator(IOptions<FileSettings> options)
        {
            RuleFor(x => x.File.Length)
                .ExclusiveBetween(0, options.Value.FileSizeLimit)
                .WithMessage(x => File.FileMaxLength);

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage(File.ImageNotFound)
                .NotImage()
                .WithMessage(File.InvalidImageSignature);
        }
    }
}