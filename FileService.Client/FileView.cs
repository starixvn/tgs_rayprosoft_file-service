#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。

using System;

namespace TGS.Apis.FileService.Clients
{
    public record FileView
    {
        public string Id { get; init; }

        public string FileName { get; init; }

        public string ContentType { get; init; }

        public decimal FileLength { get; init; }

        public DateTimeOffset CreatedOn { get; init; }

        public string ExternalUrl { get; init; }
    }
}

#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
