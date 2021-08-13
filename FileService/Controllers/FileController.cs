using System.Threading.Tasks;
using FileService.Controllers.Messages;
using FileService.DataService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers
{
    [ApiController]
    [Route("fs/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        private readonly IStationLetterService _stationLetterService;
        private readonly IUploadService _uploadService;

        public FileController(IUploadService uploadService, IStationLetterService stationLetterService,
            IPromotionService promotionService)
        {
            _uploadService = uploadService;
            _stationLetterService = stationLetterService;
            _promotionService = promotionService;
        }

        [HttpPut("{relationType}/{id}")]
        public async Task<IActionResult> UpdateRelationTypeFileAsync(string relationType, string id,
            [FromBody] UpdateFilesRequest request)
        {
            switch (relationType)
            {
                case Consts.UploadFile.Promotion:
                    await _promotionService.AddPromotionFileAsync(id, request.FileNames);
                    break;
                case Consts.UploadFile.StationLetter:
                    await _stationLetterService.AddStationLetterFileAsync(id, request.FileNames);
                    break;
            }

            return new OkResult();
        }

        [HttpGet]
        public async Task<GetFileNamesResponse> GetFileNamesFromS3Async()
        {
            var keys = await _uploadService.GetObjectsAsync();

            return new()
            {
                Names = keys
            };
        }
    }
}