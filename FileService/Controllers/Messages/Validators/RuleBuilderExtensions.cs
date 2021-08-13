using System.IO;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace FileService.Controllers.Messages.Validators
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, IFormFile> NotImage<T>(
            this IRuleBuilder<T, IFormFile> ruleBuilder)
        {
            return ruleBuilder.Must(p =>
            {
                var ext = Path.GetExtension(p.FileName).ToLowerInvariant();

                if (ImageFileSignature.FileSignature.All(x => x.Key != ext))
                {
                    return false;
                }

                using var reader = new BinaryReader(p.OpenReadStream());
                var signatures = ImageFileSignature.FileSignature[ext];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            });
        }
    }
}