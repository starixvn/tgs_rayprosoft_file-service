using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FileService;

public static class Consts
{
    public static readonly IDictionary<HealthStatus, int> HealthCheckResultStatusCodes =
        new Dictionary<HealthStatus, int>
        {
            { HealthStatus.Healthy, StatusCodes.Status200OK },
            { HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable },
            { HealthStatus.Degraded, StatusCodes.Status500InternalServerError }
        };

    public static class UploadFile
    {
        public const string ImagePattern = @"\b(http(s?):)([/|:|.|\w|-])*\.(?:jpg|gif|png|jpeg)\b";
        public const string StationLetter = "stationLetter";
        public const string Promotion = "promotion";
        public const string MessengerQRCode = "messengerQRCode";

        public static readonly string[] Types = { StationLetter, Promotion, MessengerQRCode };
    }
}