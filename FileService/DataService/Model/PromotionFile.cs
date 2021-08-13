using System;
using System.Collections.Generic;
using MongoFileMetaRepository.Model;

namespace FileService.DataService.Model
{
    public class PromotionFile : BaseEntity
    {
        public string PromotionId { get; set; }

        public ICollection<string> Files { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }
    }
}