using FileService.Core;
using MongoDB.Bson;
using File = MongoFileMetaRepository.Model.File;

namespace MongoFileMetaRepository
{
    internal class MongoFileMetaRepository : IFileMetaRepository
    {
        private readonly IMongoContext<File> _mongoContext;

        public MongoFileMetaRepository(IMongoContext<File> mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async ValueTask<string> AddFileMetaAsync(FileMeta meta)
        {
            var file = new File
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FileName = meta.FileName,
                ContentType = meta.ContentType,
                FileLength = meta.FileLength,
                CreatedOn = DateTimeOffset.Now
            };

            await _mongoContext
                .InsertAsync(file)
                .ConfigureAwait(false);

            return file.Id;
        }

        public async ValueTask DeleteFileMetaByIdsAsync(IEnumerable<string> fileIds)
            => await _mongoContext.DeleteManyAsync(fileIds.ToArray()).ConfigureAwait(false);

        public ValueTask<FileMeta> FindByIdAsync(string objectId)
        {
            var row = _mongoContext.GetQueries()
                .Where(file => file.Id == objectId)
                .Single();

            return ValueTask.FromResult(new FileMeta
            {
                Id = row.Id,
                FileName = row.FileName,
                ContentType = row.ContentType,
                FileLength = row.FileLength,
                CreatedOn = row.CreatedOn
            });
        }

        public IAsyncEnumerable<FileMeta> ListExpiredFileMetasAsync(string prefix, DateTimeOffset expireTime)
        {
            var query = _mongoContext.GetQueries().Where(row => row.CreatedOn < expireTime);

            if (!string.IsNullOrWhiteSpace(prefix))
                query = query.Where(row => row.FileName.StartsWith(prefix));

            var result = query.ToArray()
                .Select(row => new FileMeta
                {
                    Id = row.Id,
                    FileName = row.FileName,
                    ContentType = row.ContentType,
                    FileLength = row.FileLength,
                    CreatedOn = row.CreatedOn
                });

            return result.ToAsyncEnumerable();
        }

        public IAsyncEnumerable<FileMeta> ListFileMetasAsync(string prefix)
        {
            var query = _mongoContext.GetQueries().AsQueryable();

            if (!string.IsNullOrWhiteSpace(prefix))
                query = query.Where(row => row.FileName.StartsWith(prefix));

            var result = query.ToArray()
                .Select(row => new FileMeta
                {
                    Id = row.Id,
                    FileName = row.FileName,
                    ContentType = row.ContentType,
                    FileLength = row.FileLength,
                    CreatedOn = row.CreatedOn
                });

            return result.ToAsyncEnumerable();
        }
    }
}