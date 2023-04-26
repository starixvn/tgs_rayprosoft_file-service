#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。

namespace FileService.Core;

public class FileMeta
{
    public string? Id { get; set; }

    public string FileName { get; set; }

    public string ContentType { get; set; }

    public decimal FileLength { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string? ExternalUrl { get; set; }
}

#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。