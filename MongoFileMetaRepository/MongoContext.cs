using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoFileMetaRepository.Model;

namespace MongoFileMetaRepository;

public sealed class MongoContext<T> : IMongoContext<T> where T : BaseEntity
{
    private readonly IClientSessionHandle _clientSessionHandle;
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _mongoClient;

    public MongoContext(
        IOptions<FileMetaOptions> optionsAccessor,
        IMongoClient mongoClient,
        IClientSessionHandle clientSessionHandle)
    {
        _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        _clientSessionHandle = clientSessionHandle ?? throw new ArgumentNullException(nameof(clientSessionHandle));

        var options = optionsAccessor.Value;

        var builder = new MongoUrlBuilder(options.MongoConnectionString);

        _database = _mongoClient.GetDatabase(builder.DatabaseName);
        var list = _database.ListCollectionNames().ToList();
        if (!list.Contains(typeof(T).Name))
        {
            _database.CreateCollection(typeof(T).Name);
        }
    }

    private IMongoCollection<T> Collection =>
        _database.GetCollection<T>(typeof(T).Name);

    Task IMongoContext<T>.InsertAsync(T obj) => Collection.InsertOneAsync(_clientSessionHandle, obj);

    Task IMongoContext<T>.UpdateAsync(T obj)
    {
        Expression<Func<T, string>> func = f => f.Id;
        var value = (string)obj.GetType().GetProperty(func.Body.ToString().Split(".")[1])?.GetValue(obj, null)!;
        var filter = Builders<T>.Filter.Eq(func, value);

        return Collection.ReplaceOneAsync(_clientSessionHandle, filter, obj);
    }

    Task IMongoContext<T>.DeleteAsync(string id)
        => Collection.DeleteOneAsync(_clientSessionHandle, f => f.Id == id);

    Task<T> IMongoContext<T>.GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq(s => s.Id, id);
        return Collection.Find(filter).FirstOrDefaultAsync();
    }

    async Task<bool> IMongoContext<T>.CanConnection()
    {
        var names = await _mongoClient.ListDatabaseNamesAsync();
        return await names.AnyAsync();
    }

    Task IMongoContext<T>.DeleteManyAsync(string[] ids) =>
        Collection.DeleteManyAsync(x => ids.Contains(x.Id))
            .ContinueWith(x => x.Result.DeletedCount);

    Task<IAsyncCursor<T>> IMongoContext<T>.GetListAsync(Expression<Func<T, bool>> expression) =>
        Collection.FindAsync<T>(expression);

    IMongoQueryable<T> IMongoContext<T>.GetQueries() => Collection.AsQueryable();

    Task<T> IMongoContext<T>.GetByFilterAsync(FilterDefinition<T> filter) =>
        Collection.Find(filter).FirstOrDefaultAsync();

    Task<List<T>> IMongoContext<T>.GetListByFilterAsync(FilterDefinition<T> filter) =>
        Collection.Find(filter).ToListAsync();
}