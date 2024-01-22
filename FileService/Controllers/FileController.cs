using FileService.Controllers.Messages;
using FileService.DataService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers;

[ApiController]
[Route("fs/[controller]")]
public class FileController(
    IUploadService uploadService,
    IStationLetterService stationLetterService,
    IPromotionService promotionService)
    : ControllerBase
{
    [HttpPut("{relationType}/{id}")]
    public async Task<IActionResult> UpdateRelationTypeFileAsync(string relationType,
        string id,
        [FromBody] UpdateFilesRequest request)
    {
        switch (relationType)
        {
            case Consts.UploadFile.Promotion:
                await promotionService.AddPromotionFileAsync(id, request.FileNames);
                break;
            case Consts.UploadFile.StationLetter:
                await stationLetterService.AddStationLetterFileAsync(id, request.FileNames);
                break;
        }

        return new OkResult();
    }

    [HttpGet]
    public async Task<GetFileNamesResponse> GetFileNamesFromS3Async()
    {
        var keys = await uploadService.GetObjectsAsync();

        return new() { Names = keys };
    }
}