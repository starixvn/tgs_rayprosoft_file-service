using System;
using System.Collections.Generic;
using MongoFileMetaRepository.Model;

namespace FileService.DataService.Model
{
    public class StationLetterFile : BaseEntity
    {
        public string StationLetterId { get; set; }

        public ICollection<string> Files { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }
    }
}