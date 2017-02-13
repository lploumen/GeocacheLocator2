
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using GeocachingLib.DB;
using GeocachingToolbox;
using GeocachingToolbox.GeocachingCom;
using GeocachingToolbox.Opencaching;
using Newtonsoft.Json;
using SQLite.Net.Async;
using SQLiteNetExtensionsAsync.Extensions;

[assembly: InternalsVisibleTo("Tests")]
namespace GeocachingLib
{

    public class GCUserToDBUserConverter : TypeConverter<User, DBUser>
    {
        protected override DBUser ConvertCore(User user)
        {
            DBUser dbUser = new DBUser();
            dbUser.FoundGeocachesCount = user.FoundGeocachesCount;
            dbUser.Name = user.Name;
            if (user is GCUser)
                dbUser.Origin = Origin.GeocachingCom;
            else
            {
                dbUser.Origin = Origin.OpenCaching;
                dbUser.Uuid = (user as OCUser).Uuid;
            }
            return dbUser;
        }
    }


    public class DBUserToUserConverter : TypeConverter<DBUser, User>
    {
        protected override User ConvertCore(DBUser source)
        {
            switch (source.Origin)
            {
                case Origin.GeocachingCom:
                    return new GCUser(source.Name, source.FoundGeocachesCount);
                case Origin.OpenCaching:
                    return new OCUser(source.Name, source.FoundGeocachesCount, source.Uuid);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class CacheRepository : ICacheRepository
    {
        private readonly SQLiteAsyncConnection m_DB;

        public async Task Init()
        {
            await m_DB.CreateTableAsync<DBUser>();
            await m_DB.CreateTableAsync<DBGeocache>();
        }

        static CacheRepository()
        {

            Mapper.CreateMap<User, DBUser>().ConvertUsing<GCUserToDBUserConverter>();
            Mapper.CreateMap<DBUser, User>().ConvertUsing<DBUserToUserConverter>();

            Mapper.CreateMap<GCGeocache, DBGeocache>()
                .ForMember(m => m.WayPointLat, opt => opt.MapFrom(x => x.Waypoint.Latitude))
                .ForMember(m => m.WayPointLong, opt => opt.MapFrom(x => x.Waypoint.Longitude))
                .ForMember(m => m.Origin, opt => opt.UseValue(Origin.GeocachingCom))
                .ForMember(m => m.OwnerId, opt => opt.Ignore())
                .ForMember(m => m.Logs, opt => opt.ResolveUsing(x => ConvertLogsToJSon(x.Logs)));

            Mapper.CreateMap<OCGeocache, DBGeocache>()
                .ForMember(m => m.WayPointLat, opt => opt.MapFrom(x => x.Waypoint.Latitude))
                .ForMember(m => m.WayPointLong, opt => opt.MapFrom(x => x.Waypoint.Longitude))
                .ForMember(m => m.Origin, opt => opt.UseValue(Origin.OpenCaching))
                .ForMember(m => m.OwnerId, opt => opt.Ignore())
                .ForMember(m => m.DetailsUrl, opt => opt.Ignore())
                 .ForMember(m => m.Logs, opt => opt.ResolveUsing(x => ConvertLogsToJSon(x.Logs)))
                .ForMember(m => m.IsPremium, opt => opt.Ignore());

            Mapper.CreateMap<DBGeocache, GCGeocache>()
                .ForMember(m => m.Waypoint,
                    opt => opt.MapFrom(x => new Location((decimal)x.WayPointLat, (decimal)x.WayPointLong)))
                .ForMember(m => m.Logs, opt => opt.ResolveUsing(x => ConvertJSonLogsToList(x.Logs)))
                .ForMember(m => m.Owner, opt => opt.Ignore());

            Mapper.CreateMap<DBGeocache, OCGeocache>()
                .ForMember(m => m.Waypoint,
                    opt => opt.MapFrom(x => new Location((decimal)x.WayPointLat, (decimal)x.WayPointLong)))
                .ForMember(m => m.Logs, opt => opt.ResolveUsing(x => ConvertJSonLogsToList(x.Logs)))
                .ForMember(m => m.Owner, opt => opt.Ignore());

            Mapper.AssertConfigurationIsValid();
        }

        internal static string ConvertLogsToJSon(IEnumerable<Log> logs)
        {
            return JsonConvert.SerializeObject(logs);
        }

        internal static List<Log> ConvertJSonLogsToList(string json)
        {
            if (String.IsNullOrEmpty(json))
                return null;
            return JsonConvert.DeserializeObject<List<Log>>(json) as List<Log>;
        }


        public CacheRepository(SQLiteAsyncConnection db)
        {
            m_DB = db;
        }

        public static User ConvertDBUserToUser(DBUser dbUser)
        {
            return Mapper.Map<DBUser, User>(dbUser);
        }

        public static DBUser ConvertUserToDBUser(User user)
        {
            return Mapper.Map<User, DBUser>(user);
        }

        public static DBGeocache ConvertGeocacheToDbGeocache(Geocache geocache)
        {
            if (geocache is GCGeocache)
            {
                var dbGeocache = Mapper.Map<GCGeocache, DBGeocache>((GCGeocache)geocache);
                return dbGeocache;
            }
            if (geocache is OCGeocache)
            {
                var dbGeocache = Mapper.Map<OCGeocache, DBGeocache>((OCGeocache)geocache);
                return dbGeocache;
            }
            return null;
        }

        public static Geocache ConvertCacheToGeocache(DBGeocache dbGeocache)
        {
            //Geocache geocache;
            switch (dbGeocache.Origin)
            {
                case Origin.GeocachingCom:
                    return Mapper.Map<DBGeocache, GCGeocache>(dbGeocache);
                case Origin.OpenCaching:
                    return Mapper.Map<DBGeocache, OCGeocache>(dbGeocache);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<Geocache> Get(string cacheCode)
        {
            var dbGeocache = await m_DB.FindAsync<DBGeocache>(cacheCode);
            if (dbGeocache != null)
                return ConvertCacheToGeocache(dbGeocache);
            return null;
        }

        public async Task<List<Geocache>> Get(string[] cacheCodes)
        {
            if (cacheCodes?.Any() == false)
            {
                throw new ArgumentException("At least one key is required", nameof(cacheCodes));
            }
            var dbCaches = await m_DB.Table<DBGeocache>().Where(x => cacheCodes.Contains(x.Code)).ToListAsync();
            return dbCaches.Select(ConvertCacheToGeocache).ToList();
        }

        public async Task<List<Geocache>> Get()
        {
            var caches = await m_DB.Table<DBGeocache>().ToListAsync();
            var geocache = Mapper.Map<List<DBGeocache>, List<Geocache>>(caches);
            return geocache;
        }

        public async Task InsertAll(IEnumerable<Geocache> caches)
        {
            foreach (var geocache in caches)
            {
                await Insert(geocache);
            }
        }

        public async Task<IEnumerable<Geocache>> GetAsync<T>(Expression<Func<DBGeocache, bool>> predicate, Expression<Func<DBGeocache, T>> orderBy = null)
        {
            var query = m_DB.Table<DBGeocache>();
            query = query.Where(predicate);
            if (orderBy != null)
                query = query.OrderBy(orderBy);

            var caches = await query.ToListAsync();
            return caches.Select(ConvertCacheToGeocache);
        }

        public async Task Insert(Geocache cache)
        {
            var c = Mapper.Map<DBGeocache>(cache);
            var userFound = await m_DB.Table<DBUser>().Where(x => x.Name == c.Owner.Name && x.Origin == c.Origin).FirstOrDefaultAsync();
            if (userFound != null)
                c.Owner = userFound;
            await m_DB.InsertOrReplaceWithChildrenAsync(c);
        }
    }
}