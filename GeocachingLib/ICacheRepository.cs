using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GeocachingLib.DB;
using GeocachingToolbox;

namespace GeocachingLib
{
    public interface ICacheRepository
    {
        Task<List<Geocache>> Get();
        Task<Geocache> Get(string cacheCode);
        Task Insert(Geocache cache);
        Task InsertAll(IEnumerable<Geocache> caches);
        Task<IEnumerable<Geocache>> GetAsync<T>(Expression<Func<DBGeocache, bool>> predicate, Expression<Func<DBGeocache, T>> orderBy=null);
        Task<List<Geocache>> Get(string[] cacheCodes);
    }
}