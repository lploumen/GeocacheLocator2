using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeocachingLib;
using GeocachingLib.DB;
using GeocachingToolbox;
using GeocachingToolbox.GeocachingCom;
using GeocachingToolbox.Opencaching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Platform.Generic;

namespace Tests
{
    [TestClass]
    public class GeocachingLibRepositoryUnitTests
    {
        [TestMethod]
        public void TestConvertDBUserToGCUser()
        {
            DBUser dbUser = new DBUser
            {
                Name = "DBUser",
                FoundGeocachesCount = 50,
                Uuid = "4156",
                Origin = Origin.GeocachingCom
            };
            var user = CacheRepository.ConvertDBUserToUser(dbUser);
            Assert.IsInstanceOfType(user, typeof(GCUser));
            Assert.AreEqual(user.Name, dbUser.Name);
            Assert.AreEqual(user.FoundGeocachesCount, dbUser.FoundGeocachesCount);
        }

        [TestMethod]
        public void TestConvertDBUserToOCUser()
        {
            DBUser dbUser = new DBUser
            {
                Name = "DBUser",
                Origin = Origin.OpenCaching,
                FoundGeocachesCount = 50,
                Uuid = "4156",
            };
            var user = CacheRepository.ConvertDBUserToUser(dbUser);
            Assert.IsInstanceOfType(user, typeof(OCUser));
            Assert.AreEqual(user.Name, dbUser.Name);
            Assert.AreEqual(user.FoundGeocachesCount, dbUser.FoundGeocachesCount);
            Assert.AreEqual((user as OCUser).Uuid, dbUser.Uuid);

        }
        [TestMethod]
        public void TestConvertGCUserToDBUser()
        {
            GCUser gcUser = new GCUser("nico", 987);
            var dbuser = CacheRepository.ConvertUserToDBUser(gcUser);
            Assert.AreEqual(dbuser.Name, "nico");
            Assert.AreEqual(dbuser.FoundGeocachesCount, 987);
            Assert.AreEqual(dbuser.Origin, Origin.GeocachingCom);
        }

        [TestMethod]
        public void TestConvertOCUserToDBUser()
        {
            OCUser gcUser = new OCUser("nico", 987, "uuid");
            var dbuser = CacheRepository.ConvertUserToDBUser(gcUser);
            Assert.AreEqual(dbuser.Name, "nico");
            Assert.AreEqual(dbuser.FoundGeocachesCount, 987);
            Assert.AreEqual(dbuser.Origin, Origin.OpenCaching);
            Assert.AreEqual(dbuser.Uuid, "uuid");
        }

        [TestMethod]
        public void TestConvertOCGeocacheToDbGeocache()
        {
            OCGeocache geocache = new OCGeocache
            {
                Code = "123",
                Waypoint = new Location(10d, 11d),
                Hint = "coucou",
                Owner = new GCUser("nico", 99),
            };


            var cache = CacheRepository.ConvertGeocacheToDbGeocache(geocache);

            Assert.AreEqual(cache.Code, geocache.Code);
            Assert.AreEqual((float)geocache.Waypoint.Longitude, cache.WayPointLong);
            Assert.AreEqual((float)geocache.Waypoint.Latitude, cache.WayPointLat);
            Assert.AreEqual(cache.Origin, Origin.OpenCaching);
            Assert.AreEqual(cache.Owner.Name, "nico");
            Assert.AreEqual(cache.Owner.FoundGeocachesCount, 99);
        }


        [TestMethod]
        public void TestConvertGCGeocacheToCache()
        {
            GCGeocache geocache = new GCGeocache
            {
                Code = "123",
                Waypoint = new Location(10d, 11d),
                Hint = "coucou",
                IsPremium = true,
                Owner = new GCUser("nico", 99),
                DetailsUrl = "detailurl"
            };


            var cache = CacheRepository.ConvertGeocacheToDbGeocache(geocache);

            Assert.AreEqual(cache.Code, geocache.Code);
            Assert.AreEqual((float)geocache.Waypoint.Longitude, cache.WayPointLong);
            Assert.AreEqual((float)geocache.Waypoint.Latitude, cache.WayPointLat);
            Assert.AreEqual(cache.Origin, Origin.GeocachingCom);
            Assert.AreEqual(geocache.IsPremium, cache.IsPremium);
            Assert.AreEqual(cache.Owner.Name, "nico");
            Assert.AreEqual(cache.Owner.FoundGeocachesCount, 99);
            Assert.AreEqual(geocache.DetailsUrl, cache.DetailsUrl);
        }
        [TestMethod]
        public void TestConvertCacheToGCGeocache()
        {
            DBGeocache dbGeocache = new DBGeocache
            {
                Origin = Origin.GeocachingCom,
                Code = "code",
                WayPointLat = 10.1f,
                WayPointLong = 10.2f,
                IsPremium = true,
                DetailsUrl = "detail"
            };

            Geocache geocache = CacheRepository.ConvertCacheToGeocache(dbGeocache);
            Assert.IsInstanceOfType(geocache, typeof(GCGeocache));
            Assert.AreEqual(dbGeocache.Code, geocache.Code);
            Assert.AreEqual(dbGeocache.WayPointLat, (float)geocache.Waypoint.Latitude);
            Assert.AreEqual(dbGeocache.WayPointLong, (float)geocache.Waypoint.Longitude);
            Assert.AreEqual(dbGeocache.Origin, Origin.GeocachingCom);
            Assert.AreEqual(dbGeocache.IsPremium, ((GCGeocache)geocache).IsPremium);

        }
        [TestMethod]
        public void TestConvertCacheToOCGeocache()
        {
            DBGeocache dbGeocache = new DBGeocache
            {
                Origin = Origin.OpenCaching,
                Code = "code",
                WayPointLat = 10.1f,
                WayPointLong = 10.2f
            };

            Geocache geocache = CacheRepository.ConvertCacheToGeocache(dbGeocache);
            Assert.AreEqual(dbGeocache.Code, geocache.Code);
            Assert.AreEqual(dbGeocache.WayPointLat, (float)geocache.Waypoint.Latitude);
            Assert.AreEqual(dbGeocache.WayPointLong, (float)geocache.Waypoint.Longitude);
            Assert.IsInstanceOfType(geocache, typeof(OCGeocache));
        }

        CacheRepository CreateAndInitCacheRepository()
        {
            var connectionFactory = new Func<SQLiteConnectionWithLock>(() => new SQLiteConnectionWithLock(new SQLitePlatformGeneric(), new SQLiteConnectionString("geo.db", true)));
            var db = new SQLiteAsyncConnection(connectionFactory);
            CacheRepository repository = new CacheRepository(db);
            repository.Init().GetAwaiter().GetResult();
            return repository;
        }

        [TestMethod]
        [DeploymentItem("sqlite3.dll")]
        public void TestSqlite()
        {
            var repository = CreateAndInitCacheRepository();

            GCGeocache geocache = new GCGeocache
            {
                Code = "123",
                Waypoint = new Location(10d, 11d),
                Hint = "coucou",
                IsPremium = true,
                Owner = new GCUser("nico", 99),
                DetailsUrl = "detailurl",
            };

            repository.Insert(geocache).GetAwaiter().GetResult();
            var cacheFromDb = repository.Get("123").GetAwaiter().GetResult();

            Assert.AreEqual(geocache.Code, cacheFromDb.Code);
            Assert.AreEqual(geocache.IsPremium, ((GCGeocache)cacheFromDb).IsPremium);
            Assert.IsInstanceOfType(geocache, typeof(GCGeocache));
            Assert.AreEqual(geocache.DateHidden, cacheFromDb.DateHidden);
            Assert.AreEqual(geocache.Description, cacheFromDb.Description);
        }

        [TestMethod]
        public void TestSerializeLogsToJSon()
        {
            List<Log> gcLogs = new List<Log>
            {
                new Log()
                {
                    Comment = "TFTC",
                    Date = new DateTime(2010, 3, 30),
                    LogType = GeocacheLogType.Found,
                    Username = "LPL"
                },
                 new Log()
                {
                    Comment = "I cannot find it",
                    Date = new DateTime(2013, 10, 30),
                    LogType = GeocacheLogType.WriteNote,
                    Username = "Oufti"
                }
            };
            GCGeocache gc = new GCGeocache("1234")
            {
                Logs = gcLogs,
                Owner = new GCUser("123", 1)
            };
            var dbGeocache = CacheRepository.ConvertGeocacheToDbGeocache(gc);
            var geocache = CacheRepository.ConvertCacheToGeocache(dbGeocache);

            var reconstructedLogs = CacheRepository.ConvertJSonLogsToList(dbGeocache.Logs);
            Assert.AreEqual(reconstructedLogs.Count, gcLogs.Count);
            
        }
    }
}
