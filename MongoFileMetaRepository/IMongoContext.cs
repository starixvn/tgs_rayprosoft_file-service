using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoFileMetaRepository
{
    public interface IMongoContext<T>
    {
        Task InsertAsync(T obj);

        Task UpdateAsync(T obj);

        Task DeleteAsync(string id);

        Task DeleteManyAsync(string[] ids);

        Task<T> GetByIdAsync(string id);

        IMongoQueryable<T> GetQueries();

        Task<IAsyncCursor<T>> GetListAsync(Expression<Func<T, bool>> expression);

        Task<bool> CanConnection();

        Task<List<T>> GetListByFilterAsync(FilterDefinition<T> filter);

        Task<T> GetByFilterAsync(FilterDefinition<T> filter);
    }
}